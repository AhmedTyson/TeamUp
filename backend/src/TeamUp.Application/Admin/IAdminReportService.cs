// Date: 2026-11-24
// Purpose: Platform governance PDF generation, executive email delivery, and editorial email template preview contract.

namespace TeamUp.Application.Admin;

public sealed record AdminReportSummaryDto(
    string ReportPeriod,
    DateTime GeneratedAtUtc,
    string GeneratedByEmail,
    int ActiveStudents,
    int SoftDeletedStudents,
    int ActiveUniversities,
    int ActiveFaculties,
    int ActiveProjects,
    int RecruitingProjects,
    int ActiveSkills,
    int ActiveOrganizations,
    int ActiveEvents,
    int TotalAuditLogs,
    string FileName);

public sealed record GeneratedAdminReport(
    byte[] PdfBytes,
    AdminReportSummaryDto Summary);

public sealed record AdminReportEmailResultDto(
    string RecipientEmail,
    string FileName,
    int PdfSizeBytes,
    AdminReportSummaryDto Summary);

public interface IAdminReportService
{
    Task<GeneratedAdminReport> GeneratePlatformSummaryPdfAsync(
        int actorStudentId,
        string? reportPeriod = null,
        CancellationToken ct = default);

    Task<AdminReportEmailResultDto> SendPlatformSummaryEmailAsync(
        int actorStudentId,
        string? recipientEmail = null,
        string? reportPeriod = null,
        CancellationToken ct = default);

    Task<string> RenderTemplatePreviewAsync(
        string templateName,
        CancellationToken ct = default);
}
