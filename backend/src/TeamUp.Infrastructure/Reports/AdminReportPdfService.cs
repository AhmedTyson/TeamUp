// Date: 2026-11-24
// Purpose: QuestPDF vector governance report generator, executive email attachment dispatcher, and 7-template live HTML previewer.

using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeamUp.Application.Admin;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Gamification;
using TeamUp.Domain.Enums;

namespace TeamUp.Infrastructure.Reports;

public sealed class AdminReportPdfService(
    IAppDbContext db,
    IGamificationService gamification,
    ITemplateRenderer templateRenderer,
    IEmailSender emailSender,
    IAuditLogService auditLog,
    IOptions<EmailOptions> emailOptions) : IAdminReportService
{
    private readonly EmailOptions _email = emailOptions.Value;

    public async Task<GeneratedAdminReport> GeneratePlatformSummaryPdfAsync(
        int actorStudentId,
        string? reportPeriod = null,
        CancellationToken ct = default)
    {
        EditorialPdfTheme.EnsureInitialized();

        var generatedAt = DateTime.UtcNow;
        var period = string.IsNullOrWhiteSpace(reportPeriod)
            ? $"EXECUTIVE LEDGER · {generatedAt:MMMM yyyy}".ToUpperInvariant()
            : reportPeriod.Trim();

        var actor = actorStudentId > 0
            ? await db.Students.AsNoTracking()
                .Where(s => s.StudentId == actorStudentId)
                .Select(s => new { s.Email, FullName = s.FirstName + " " + s.LastName })
                .FirstOrDefaultAsync(ct)
            : null;
        var actorEmail = actor?.Email ?? "admin@teamup.local";
        var fileName = $"TeamUp_Platform_Governance_{generatedAt:yyyy-MM-dd}.pdf";

        var activeStudents = await db.Students.CountAsync(ct);
        var softDeletedStudents = await db.Students.IgnoreQueryFilters().CountAsync(s => s.DeletedAt != null, ct);
        var activeUniversities = await db.Universities.CountAsync(ct);
        var activeFaculties = await db.Faculties.CountAsync(ct);
        var activeProjects = await db.Projects.CountAsync(ct);
        var recruitingProjects = await db.Projects.CountAsync(p => p.Status == ProjectStatus.Recruiting, ct);
        var activeSkills = await db.Skills.CountAsync(ct);
        var activeOrganizations = await db.Organizations.CountAsync(ct);
        var activeEvents = await db.Events.CountAsync(ct);
        var totalAuditLogs = await db.AuditLogs.CountAsync(ct);

        var roleRows = await db.Students
            .AsNoTracking()
            .GroupBy(s => s.Role)
            .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        var universityRows = await db.Universities
            .AsNoTracking()
            .Select(u => new
            {
                u.Name,
                u.City,
                FacultyCount = u.Faculties.Count(),
                StudentCount = u.Faculties.SelectMany(f => f.Students).Count()
            })
            .OrderByDescending(u => u.StudentCount)
            .ThenBy(u => u.Name)
            .Take(10)
            .ToListAsync(ct);

        var scoreByStudent = await db.ScoreTransactions
            .AsNoTracking()
            .GroupBy(t => t.StudentId)
            .Select(g => new { StudentId = g.Key, TotalPoints = g.Sum(x => x.Points) })
            .ToDictionaryAsync(x => x.StudentId, x => x.TotalPoints, ct);

        var studentMeta = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.StudentId,
                FullName = s.FirstName + " " + s.LastName,
                FacultyCode = s.Faculty.Code
            })
            .ToListAsync(ct);

        var leaderboard = studentMeta
            .Select(s =>
            {
                var xp = scoreByStudent.GetValueOrDefault(s.StudentId, 0);
                var metrics = gamification.CalculateLevelMetrics(s.StudentId, xp);
                return new { s.StudentId, s.FullName, s.FacultyCode, Xp = xp, metrics.Level };
            })
            .OrderByDescending(x => x.Xp)
            .ThenBy(x => x.StudentId)
            .Take(10)
            .Select((row, idx) => new { Rank = idx + 1, row.FullName, row.FacultyCode, row.Level, row.Xp })
            .ToList();

        var recruitingList = await db.Projects
            .AsNoTracking()
            .Where(p => p.Status == ProjectStatus.Recruiting)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .Select(p => new
            {
                p.Title,
                Lead = p.Creator.FirstName + " " + p.Creator.LastName,
                Members = p.Members.Count(),
                p.MaxTeamSize
            })
            .ToListAsync(ct);

        var skillDemand = await db.ProjectRequiredSkills
            .AsNoTracking()
            .GroupBy(prs => new { prs.SkillId, prs.Skill.Name, prs.Skill.Category })
            .Select(g => new { g.Key.Name, g.Key.Category, DemandCount = g.Count() })
            .OrderByDescending(x => x.DemandCount)
            .ThenBy(x => x.Name)
            .Take(8)
            .ToListAsync(ct);

        var recentAudits = await db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.OccurredAt)
            .ThenByDescending(a => a.AuditLogId)
            .Take(15)
            .Select(a => new
            {
                a.OccurredAt,
                a.Action,
                Target = a.EntityType + ":" + a.EntityId,
                Actor = a.ActorEmail ?? "system",
                Ip = a.ClientIp ?? "-"
            })
            .ToListAsync(ct);

        var summary = new AdminReportSummaryDto(
            period,
            generatedAt,
            actorEmail,
            activeStudents,
            softDeletedStudents,
            activeUniversities,
            activeFaculties,
            activeProjects,
            recruitingProjects,
            activeSkills,
            activeOrganizations,
            activeEvents,
            totalAuditLogs,
            fileName);

        var pdfBytes = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(24);
                page.MarginHorizontal(26);
                page.PageColor(EditorialPdfTheme.Paper);
                page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(9.5f).FontColor(EditorialPdfTheme.InkPrimary));

                page.Header().ShowOnce().Element(c => EditorialPdfTheme.ComposeMasthead(
                    c,
                    "GOVERNANCE / EXECUTIVE PDF",
                    "TeamUp Platform Governance Summary",
                    $"{period} · Actor: {actorEmail}",
                    $"Generated {generatedAt:yyyy-MM-dd HH:mm} UTC"));

                page.Content().Column(col =>
                {
                    col.Item().Element(c => EditorialPdfTheme.ComposeSectionHeader(c, "01", "Executive Platform Metrics"));
                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.Spacing(8);
                        row.RelativeItem().Element(c => EditorialPdfTheme.ComposeKpiTile(c, "Active Students", activeStudents.ToString("N0"), $"{softDeletedStudents} archived"));
                        row.RelativeItem().Element(c => EditorialPdfTheme.ComposeKpiTile(c, "Universities", activeUniversities.ToString("N0"), $"{activeFaculties} faculties"));
                        row.RelativeItem().Element(c => EditorialPdfTheme.ComposeKpiTile(c, "Projects", activeProjects.ToString("N0"), $"{recruitingProjects} recruiting", accentForest: true));
                        row.RelativeItem().Element(c => EditorialPdfTheme.ComposeKpiTile(c, "Events & Orgs", activeEvents.ToString("N0"), $"{activeOrganizations} organizations"));
                    });

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.Spacing(8);
                        row.RelativeItem().Element(c => EditorialPdfTheme.ComposeKpiTile(c, "Master Skills", activeSkills.ToString("N0")));
                        row.RelativeItem().Element(c => EditorialPdfTheme.ComposeKpiTile(c, "Audit Entries", totalAuditLogs.ToString("N0"), "Immutable STRIDE THR-R-01"));
                        row.RelativeItem().Element(c => EditorialPdfTheme.ComposeKpiTile(c, "Soft-Deleted Users", softDeletedStudents.ToString("N0"), "Recoverable via Admin Console"));
                        row.RelativeItem().Element(c => EditorialPdfTheme.ComposeKpiTile(c, "Governance Actor", actor?.FullName ?? "Platform Admin", actorEmail));
                    });

                    col.Item().Element(c => EditorialPdfTheme.ComposeSectionHeader(c, "02", "Role Distribution & Campus Enrollment"));
                    col.Item().PaddingTop(8).Row(split =>
                    {
                        split.Spacing(10);
                        split.RelativeItem(2).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(1);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Role"));
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Count", alignRight: true));
                            });
                            for (var i = 0; i < roleRows.Count; i++)
                            {
                                var r = roleRows[i];
                                var alt = i % 2 == 1;
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, r.Role, alt, bold: true));
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, r.Count.ToString("N0"), alt, alignRight: true));
                            }
                            if (roleRows.Count == 0)
                            {
                                table.Cell().ColumnSpan(2).Element(c => EditorialPdfTheme.ComposeTableCell(c, "No active role records."));
                            }
                        });

                        split.RelativeItem(4).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(4);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "University"));
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "City"));
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Fac", alignRight: true));
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Stud", alignRight: true));
                            });
                            for (var i = 0; i < universityRows.Count; i++)
                            {
                                var u = universityRows[i];
                                var alt = i % 2 == 1;
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, u.Name, alt, bold: true));
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, u.City, alt));
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, u.FacultyCount.ToString(), alt, alignRight: true));
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, u.StudentCount.ToString(), alt, bold: true, alignRight: true));
                            }
                            if (universityRows.Count == 0)
                            {
                                table.Cell().ColumnSpan(4).Element(c => EditorialPdfTheme.ComposeTableCell(c, "No universities registered."));
                            }
                        });
                    });

                    col.Item().Element(c => EditorialPdfTheme.ComposeSectionHeader(c, "03", "Top 10 Campus Leaderboard Snapshot"));
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(40);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.ConstantColumn(45);
                            cols.ConstantColumn(65);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Rank"));
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Student"));
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Faculty"));
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Lvl", alignRight: true));
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Total XP", alignRight: true));
                        });
                        for (var i = 0; i < leaderboard.Count; i++)
                        {
                            var row = leaderboard[i];
                            var alt = i % 2 == 1;
                            var rankColor = row.Rank == 1 ? EditorialPdfTheme.ForestInk : EditorialPdfTheme.InkPrimary;
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, $"#{row.Rank:D2}", alt, bold: true, textColor: rankColor));
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, row.FullName, alt, bold: row.Rank == 1));
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, row.FacultyCode, alt, textColor: EditorialPdfTheme.InkSecondary));
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, row.Level.ToString(), alt, alignRight: true));
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, row.Xp.ToString("N0"), alt, bold: true, alignRight: true));
                        }
                        if (leaderboard.Count == 0)
                        {
                            table.Cell().ColumnSpan(5).Element(c => EditorialPdfTheme.ComposeTableCell(c, "No student XP entries recorded yet."));
                        }
                    });

                    col.Item().Element(c => EditorialPdfTheme.ComposeSectionHeader(c, "04", "Recruiting Squads & Top Requested Skills"));
                    col.Item().PaddingTop(8).Row(split =>
                    {
                        split.Spacing(10);
                        split.RelativeItem(3).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(2);
                                cols.ConstantColumn(55);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Recruiting Squad"));
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Lead"));
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Roster", alignRight: true));
                            });
                            for (var i = 0; i < recruitingList.Count; i++)
                            {
                                var p = recruitingList[i];
                                var alt = i % 2 == 1;
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, p.Title, alt, bold: true));
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, p.Lead, alt));
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, $"{p.Members}/{p.MaxTeamSize}", alt, alignRight: true));
                            }
                            if (recruitingList.Count == 0)
                            {
                                table.Cell().ColumnSpan(3).Element(c => EditorialPdfTheme.ComposeTableCell(c, "No recruiting squads currently open."));
                            }
                        });

                        split.RelativeItem(2).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(2);
                                cols.ConstantColumn(45);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Skill"));
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Domain"));
                                h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Reqs", alignRight: true));
                            });
                            for (var i = 0; i < skillDemand.Count; i++)
                            {
                                var s = skillDemand[i];
                                var alt = i % 2 == 1;
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, s.Name, alt, bold: true));
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, s.Category, alt));
                                table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, s.DemandCount.ToString(), alt, alignRight: true));
                            }
                            if (skillDemand.Count == 0)
                            {
                                table.Cell().ColumnSpan(3).Element(c => EditorialPdfTheme.ComposeTableCell(c, "No project skill tags recorded."));
                            }
                        });
                    });

                    col.Item().Element(c => EditorialPdfTheme.ComposeSectionHeader(c, "05", "Immutable Audit Trail (Latest 15 Events)"));
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(96);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.ConstantColumn(72);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Timestamp UTC"));
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Action"));
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Target"));
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "Actor"));
                            h.Cell().Element(c => EditorialPdfTheme.ComposeTableHeaderCell(c, "IP"));
                        });
                        for (var i = 0; i < recentAudits.Count; i++)
                        {
                            var a = recentAudits[i];
                            var alt = i % 2 == 1;
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, a.OccurredAt.ToString("yyyy-MM-dd HH:mm"), alt));
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, a.Action, alt, bold: true));
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, a.Target, alt));
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, a.Actor, alt));
                            table.Cell().Element(c => EditorialPdfTheme.ComposeTableCell(c, a.Ip, alt));
                        }
                        if (recentAudits.Count == 0)
                        {
                            table.Cell().ColumnSpan(5).Element(c => EditorialPdfTheme.ComposeTableCell(c, "No audit events recorded."));
                        }
                    });
                });

                page.Footer().Element(EditorialPdfTheme.ComposeFooter);
            });
        }).GeneratePdf();

        return new GeneratedAdminReport(pdfBytes, summary);
    }

    public async Task<AdminReportEmailResultDto> SendPlatformSummaryEmailAsync(
        int actorStudentId,
        string? recipientEmail = null,
        string? reportPeriod = null,
        CancellationToken ct = default)
    {
        var generated = await GeneratePlatformSummaryPdfAsync(actorStudentId, reportPeriod, ct);
        var summary = generated.Summary;
        var targetEmail = string.IsNullOrWhiteSpace(recipientEmail)
            ? summary.GeneratedByEmail
            : recipientEmail.Trim();

        var baseUrl = string.IsNullOrWhiteSpace(_email.FrontendBaseUrl)
            ? "https://teamup-ieee.vercel.app"
            : _email.FrontendBaseUrl.Trim().TrimEnd('/');

        var htmlBody = await templateRenderer.RenderEmailAsync(
            "AdminReportEmail.html",
            new Dictionary<string, string>
            {
                ["AdminName"] = targetEmail,
                ["ReportPeriod"] = summary.ReportPeriod,
                ["GeneratedAtUtc"] = summary.GeneratedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'"),
                ["TotalStudents"] = summary.ActiveStudents.ToString("N0"),
                ["ActiveProjects"] = summary.ActiveProjects.ToString("N0"),
                ["ActiveEvents"] = summary.ActiveEvents.ToString("N0"),
                ["AuditEventsCount"] = summary.TotalAuditLogs.ToString("N0"),
                ["AttachmentFileName"] = summary.FileName,
                ["AdminConsoleUrl"] = $"{baseUrl}/admin/communications"
            },
            ct);

        var plainText = $"TeamUp Platform Governance Summary ({summary.ReportPeriod})\nStudents: {summary.ActiveStudents} | Projects: {summary.ActiveProjects} | Events: {summary.ActiveEvents} | Audit Entries: {summary.TotalAuditLogs}\nAttached PDF: {summary.FileName}";

        await emailSender.SendTemplatedEmailAsync(
            new EmailDispatchRequest(
                targetEmail,
                $"TeamUp Governance PDF Report — {summary.GeneratedAtUtc:yyyy-MM-dd}",
                htmlBody,
                plainText,
                [
                    new EmailAttachment(summary.FileName, "application/pdf", generated.PdfBytes)
                ]),
            ct);

        await auditLog.RecordAsync(
            "GovernanceReportEmailed",
            "PlatformReport",
            summary.FileName,
            newValues: new
            {
                RecipientEmail = targetEmail,
                summary.ReportPeriod,
                PdfSizeBytes = generated.PdfBytes.Length
            },
            actorStudentIdOverride: actorStudentId > 0 ? actorStudentId : null,
            ct: ct);

        return new AdminReportEmailResultDto(targetEmail, summary.FileName, generated.PdfBytes.Length, summary);
    }

    public async Task<string> RenderTemplatePreviewAsync(string templateName, CancellationToken ct = default)
    {
        var cleaned = Path.GetFileName(templateName.Trim());
        if (!cleaned.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            cleaned += ".html";

        var available = templateRenderer.GetAvailableEmailTemplates();
        var canonical = available.FirstOrDefault(t => string.Equals(t, cleaned, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException($"Email template '{templateName}' was not found.");

        var baseUrl = string.IsNullOrWhiteSpace(_email.FrontendBaseUrl)
            ? "https://teamup-ieee.vercel.app"
            : _email.FrontendBaseUrl.Trim().TrimEnd('/');

        return canonical switch
        {
            "PasswordReset.html" => await templateRenderer.RenderEmailAsync(canonical, new Dictionary<string, string>
            {
                ["Email"] = "salma@cu.edu.eg",
                ["ResetUrl"] = $"{baseUrl}/auth/reset-password?email=salma%40cu.edu.eg&token=sample_token_preview",
                ["ResetToken"] = "9f8a7b6c5d4e3f2a1b0c9d8e7f6a5b4c",
                ["ExpiresMinutes"] = "15"
            }, ct),

            "EmailVerification.html" => await templateRenderer.RenderEmailAsync(canonical, new Dictionary<string, string>
            {
                ["FirstName"] = "Salma",
                ["Email"] = "salma@cu.edu.eg",
                ["UniversityName"] = "Cairo University",
                ["FacultyName"] = "Faculty of Computers and Artificial Intelligence",
                ["VerifyUrl"] = $"{baseUrl}/auth/verify?code=491028",
                ["VerificationCode"] = "491-028",
                ["ExpiresMinutes"] = "15"
            }, ct),

            "EventNotification.html" => await templateRenderer.RenderEmailAsync(canonical, new Dictionary<string, string>
            {
                ["RecipientName"] = "Omar Mostafa",
                ["StatusLabel"] = "CONFIRMED SEAT",
                ["EventMonth"] = "DEC",
                ["EventDay"] = "14",
                ["EventTitle"] = "IEEE Cairo Embedded & Robotics Summit",
                ["OrganizationName"] = "IEEE Cairo University Student Branch",
                ["EventDateRange"] = "2026-12-14 10:00 - 15:00 UTC",
                ["VenueLocation"] = "FCAI Main Auditorium, Giza",
                ["SummaryText"] = "UI Template Preview (M3-isolated): rotating 60s QR check-in enabled at Hall A.",
                ["EventUrl"] = $"{baseUrl}/events/12",
                ["PreferencesUrl"] = $"{baseUrl}/profile/me"
            }, ct),

            "GeneralNotification.html" => await templateRenderer.RenderEmailAsync(canonical, new Dictionary<string, string>
            {
                ["RecipientName"] = "Hana El-Sayed",
                ["CategoryLabel"] = "APPLICATION REVIEWED",
                ["NotificationTitle"] = "You joined Autonomous Rover Telemetry",
                ["SummaryMessage"] = "Your squad application was accepted by the team lead. Sprint tasks and roster tools are unlocked.",
                ["ContextLabel"] = "PROJECT",
                ["ContextValue"] = "Autonomous Rover Telemetry",
                ["TimestampUtc"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'"),
                ["ActionLabel"] = "OPEN PROJECT WORKSPACE",
                ["ActionUrl"] = $"{baseUrl}/projects/7",
                ["PreferencesUrl"] = $"{baseUrl}/profile/me"
            }, ct),

            "LeaderboardDigest.html" => await templateRenderer.RenderEmailAsync(
                canonical,
                new Dictionary<string, string>
                {
                    ["IssueNumber"] = $"ISSUE {DateTime.UtcNow:yyyyMMdd}",
                    ["PeriodLabel"] = "WEEKLY CAMPUS DIGEST · PREVIEW",
                    ["RecipientName"] = "Salma Khaled",
                    ["MyRank"] = "1",
                    ["MyXp"] = "2,840",
                    ["MyLevel"] = "8",
                    ["LeaderboardUrl"] = $"{baseUrl}/leaderboard",
                    ["PreferencesUrl"] = $"{baseUrl}/profile/me"
                },
                new Dictionary<string, SafeHtmlFragment>
                {
                    ["LeaderboardRowsHtml"] = new SafeHtmlFragment(
                        BuildPreviewLeaderboardRows()),
                    ["NewsItemsHtml"] = new SafeHtmlFragment(
                        "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border:1px solid #E2E6E0;background-color:#FFFFFF;\"><tr><td style=\"padding:14px 16px;\"><p style=\"margin:0 0 4px;font-family:'SFMono-Regular',Consolas,monospace;font-size:10px;letter-spacing:0.1em;color:#15733F;font-weight:700;text-transform:uppercase;\">COMPETITION &middot; FCAI HALL A</p><p style=\"margin:0 0 4px;font-size:15px;font-weight:700;color:#14151C;\">Spring Inter-University Hackathon — Team Registration Open</p><p style=\"margin:0;font-size:13px;line-height:1.5;color:#4B5260;\">Form squads of 3 to 5 students across Engineering and Computer Science tracks before Friday.</p></td></tr></table>")
                },
                ct),

            "ProjectMatchDigest.html" => await templateRenderer.RenderEmailAsync(
                canonical,
                new Dictionary<string, string>
                {
                    ["RecipientName"] = "Salma Khaled",
                    ["MatchCount"] = "2",
                    ["StudentSkillsSummary"] = "Python · Machine Learning · React · TypeScript",
                    ["GeneratedDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    ["ProjectsUrl"] = $"{baseUrl}/projects",
                    ["PreferencesUrl"] = $"{baseUrl}/profile/me"
                },
                new Dictionary<string, SafeHtmlFragment>
                {
                    ["MatchedProjectCardsHtml"] = new SafeHtmlFragment(BuildPreviewMatchCard(baseUrl))
                },
                ct),

            "AdminReportEmail.html" => await templateRenderer.RenderEmailAsync(canonical, new Dictionary<string, string>
            {
                ["AdminName"] = "Platform Admin",
                ["ReportPeriod"] = $"EXECUTIVE LEDGER · {DateTime.UtcNow:MMMM yyyy}".ToUpperInvariant(),
                ["GeneratedAtUtc"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'"),
                ["TotalStudents"] = "412",
                ["ActiveProjects"] = "28",
                ["ActiveEvents"] = "9",
                ["AuditEventsCount"] = "15",
                ["AttachmentFileName"] = $"TeamUp_Platform_Governance_{DateTime.UtcNow:yyyy-MM-dd}.pdf",
                ["AdminConsoleUrl"] = $"{baseUrl}/admin/communications"
            }, ct),

            _ => throw new NotFoundException($"Email template '{templateName}' was not found.")
        };
    }

    private static string BuildPreviewLeaderboardRows()
    {
        var sample = new[]
        {
            ("#01", "Salma Khaled", "FCAI", "8", "2,840", true),
            ("#02", "Omar Mostafa", "ENG_CAPU", "7", "2,490", false),
            ("#03", "Youssef Adel", "CIS", "7", "2,310", false)
        };
        var sb = new StringBuilder();
        for (var i = 0; i < sample.Length; i++)
        {
            var (rank, name, fac, lvl, xp, hi) = sample[i];
            var bg = hi ? "#EDF8F1" : i % 2 == 1 ? "#F8F9F7" : "#FFFFFF";
            var rc = hi ? "#15733F" : "#14151C";
            sb.Append(
                $"<tr style=\"background-color:{bg};\">"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-size:12px;font-weight:700;color:{rc};\">{rank}</td>"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-size:13px;font-weight:600;color:#14151C;\">{WebUtility.HtmlEncode(name)}</td>"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;color:#6B7280;\">{WebUtility.HtmlEncode(fac)}</td>"
                + $"<td align=\"right\" style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-size:12px;color:#4B5260;\">{lvl}</td>"
                + $"<td align=\"right\" style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-size:12px;font-weight:700;color:#14151C;\">{xp}</td>"
                + "</tr>");
        }
        return sb.ToString();
    }

    private static string BuildPreviewMatchCard(string baseUrl) =>
        "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 18px;border:1px solid #E2E6E0;background-color:#FFFFFF;\">"
        + "<tr><td style=\"padding:18px 20px;\">"
        + "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 10px;\"><tr>"
        + "<td align=\"left\"><span style=\"display:inline-block;padding:3px 9px;background-color:#14151C;color:#D3FB52;font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;font-weight:700;border-radius:2px;\">92% MATCH</span></td>"
        + "<td align=\"right\" style=\"font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;color:#15733F;font-weight:700;\">2 / 5 spots open</td>"
        + "</tr></table>"
        + "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 12px;background-color:#E5E7EB;height:4px;\"><tr>"
        + "<td width=\"92%\" style=\"background-color:#15733F;height:4px;font-size:0;line-height:4px;\">&nbsp;</td>"
        + "<td style=\"height:4px;font-size:0;line-height:4px;\">&nbsp;</td></tr></table>"
        + "<h2 style=\"margin:0 0 4px;font-size:18px;font-weight:700;color:#14151C;\">Autonomous Campus Shuttle Vision</h2>"
        + "<p style=\"margin:0 0 10px;font-size:12px;color:#6B7280;\">Lead: <strong style=\"color:#14151C;\">Salma Khaled</strong> &middot; Cairo University &middot; FCAI</p>"
        + "<p style=\"margin:0 0 12px;font-size:14px;line-height:1.5;color:#4B5260;\">Real-time pedestrian and shuttle lane detection pipeline deployed on edge nodes.</p>"
        + $"<table role=\"presentation\" cellspacing=\"0\" cellpadding=\"0\"><tr><td style=\"background-color:#14151C;border-radius:2px;\"><a href=\"{WebUtility.HtmlEncode(baseUrl)}/projects/14\" style=\"display:inline-block;padding:10px 18px;font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;font-weight:700;letter-spacing:0.1em;text-transform:uppercase;color:#D3FB52;text-decoration:none;\">INSPECT &amp; APPLY</a></td></tr></table>"
        + "</td></tr></table>";
}
