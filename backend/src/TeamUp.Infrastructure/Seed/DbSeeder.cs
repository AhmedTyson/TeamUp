// Date: 2026-11-25
// Purpose: Idempotent JSON-driven seeder for the Egyptian campus ecosystem enforcing CIA + AAA invariants across Development and Production.

using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Seed.Models;

namespace TeamUp.Infrastructure.Seed;

/// <summary>Idempotent, CIA/AAA-hardened ecosystem catalog & bootstrap seeder.</summary>
public static class DbSeeder
{
    public const string EcosystemSeedAction = "Seed.EgyptEcosystemSynced";
    public const string EcosystemSeedEntityType = "SystemSeed";

    /// <summary>
    /// Seeds the full Egyptian campus ecosystem catalog and ensures the bootstrap PlatformAdmin account exists.
    /// Outside Development (<paramref name="isDevelopment"/> false), existing passwords are never overwritten,
    /// no demo OrgAdmin credential is created, and all showcase accounts receive unique unusable random hashes.
    /// </summary>
    public static async Task SeedAsync(
        AppDbContext db,
        string adminPasswordHash,
        bool isDevelopment = false,
        CancellationToken ct = default)
    {
        await SyncEcosystemCatalogAsync(
            db,
            isDevelopment: isDevelopment,
            devPasswordHash: isDevelopment ? adminPasswordHash : null,
            forceSync: true,
            ct: ct);

        var firstFaculty = await db.Faculties.OrderBy(f => f.FacultyId).FirstAsync(ct);

        var existingAdmin = await db.Students.FirstOrDefaultAsync(s => s.Email == "admin@teamup.local", ct);
        if (existingAdmin is null)
        {
            var admin = new Student
            {
                Email = "admin@teamup.local",
                PasswordHash = adminPasswordHash,
                FirstName = "Platform",
                LastName = "Admin",
                AcademicYear = 4,
                Role = UserRole.PlatformAdmin,
                FacultyId = firstFaculty.FacultyId,
                SecurityStamp = Guid.NewGuid(),
            };
            db.Students.Add(admin);
            await db.SaveChangesAsync(ct);

            db.AuditLogs.Add(new AuditLog
            {
                ActorStudentId = admin.StudentId,
                ActorEmail = admin.Email,
                Action = "SeedInitialized",
                EntityType = "University",
                EntityId = firstFaculty.UniversityId.ToString(),
                OldValues = null,
                NewValues = "{\"AdminEmail\":\"admin@teamup.local\"}",
                ClientIp = "127.0.0.1",
            });
            await db.SaveChangesAsync(ct);
        }
        else if (isDevelopment)
        {
            existingAdmin.PasswordHash = adminPasswordHash;
            await db.SaveChangesAsync(ct);
        }

        if (!isDevelopment)
            return;

        // Development-only convenience OrgAdmin wired as IEEE Helwan Chair for local testing workflows.
        var existingOrgAdmin = await db.Students.FirstOrDefaultAsync(s => s.Email == "nour@cu.edu.eg", ct);
        if (existingOrgAdmin is null)
        {
            existingOrgAdmin = new Student
            {
                Email = "nour@cu.edu.eg",
                PasswordHash = adminPasswordHash,
                FirstName = "Nour",
                LastName = "Hassan",
                AcademicYear = 3,
                Role = UserRole.OrgAdmin,
                FacultyId = firstFaculty.FacultyId,
                SecurityStamp = Guid.NewGuid(),
            };
            db.Students.Add(existingOrgAdmin);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            existingOrgAdmin.PasswordHash = adminPasswordHash;
            await db.SaveChangesAsync(ct);
        }

        var ieeeHelwan = await db.Organizations.FirstOrDefaultAsync(o => o.Name == "IEEE Helwan Student Branch", ct);
        if (ieeeHelwan is not null &&
            !await db.OrganizationMembers.IgnoreQueryFilters()
                .AnyAsync(m => m.OrganizationId == ieeeHelwan.OrganizationId && m.StudentId == existingOrgAdmin.StudentId, ct))
        {
            db.OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationId = ieeeHelwan.OrganizationId,
                StudentId = existingOrgAdmin.StudentId,
                Role = OrganizationRoles.Chair,
                HierarchyTier = OrganizationHierarchyTier.HighBoard,
                JoinedAt = DateTime.UtcNow.AddDays(-45),
            });
            await db.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Synchronizes the embedded Egyptian campus catalog (universities, faculties, skills, badges, community profiles,
    /// organizations, 4-tier hierarchies, projects, events, and recruitment stages) idempotently in any environment.
    /// Production accounts receive unique per-account cryptographically random unusable password hashes.
    /// </summary>
    public static async Task SyncEcosystemCatalogAsync(
        AppDbContext db,
        bool isDevelopment = false,
        string? devPasswordHash = null,
        bool forceSync = false,
        CancellationToken ct = default)
    {
        var data = SeedDataLoader.Load();

        if (!forceSync && !isDevelopment)
        {
            var alreadySynced = await db.AuditLogs.AnyAsync(
                a => a.EntityType == EcosystemSeedEntityType
                     && a.EntityId == data.Version
                     && a.Action == EcosystemSeedAction,
                ct);
            if (alreadySynced)
                return;
        }

        await SyncUniversitiesAndFacultiesAsync(db, data.Universities, ct);
        await SyncSkillsAndBadgesAsync(db, data.Skills, data.Badges, ct);

        var effectiveDevHash = isDevelopment && !string.IsNullOrWhiteSpace(devPasswordHash) ? devPasswordHash : null;
        var studentsByEmail = await SyncStudentsAsync(db, data.Students, effectiveDevHash, resetExistingPasswords: effectiveDevHash is not null, ct);

        var unisByName = ToSafeDictionary(
            await db.Universities.IgnoreQueryFilters().ToListAsync(ct),
            u => u.Name,
            u => u.DeletedAt is null);

        var orgsByName = await SyncOrganizationsAsync(db, data.Organizations, unisByName, studentsByEmail, ct);
        await SyncProjectsAsync(db, data.Projects, studentsByEmail, ct);
        await SyncEventsAsync(db, data.Events, orgsByName, studentsByEmail, ct);
        await SyncMembershipApplicationsAsync(db, data.MembershipApplications, orgsByName, studentsByEmail, ct);

        var auditExists = await db.AuditLogs.AnyAsync(
            a => a.EntityType == EcosystemSeedEntityType
                 && a.EntityId == data.Version
                 && a.Action == EcosystemSeedAction,
            ct);

        if (!auditExists)
        {
            db.AuditLogs.Add(new AuditLog
            {
                ActorStudentId = null,
                ActorEmail = "system@teamup.local",
                Action = EcosystemSeedAction,
                EntityType = EcosystemSeedEntityType,
                EntityId = data.Version,
                OldValues = null,
                NewValues = JsonSerializer.Serialize(new
                {
                    version = data.Version,
                    universities = data.Universities.Count,
                    faculties = data.Universities.Sum(u => u.Faculties.Count),
                    skills = data.Skills.Count,
                    badges = data.Badges.Count,
                    students = data.Students.Count,
                    organizations = data.Organizations.Count,
                    projects = data.Projects.Count,
                    events = data.Events.Count,
                    membershipApplications = data.MembershipApplications.Count,
                }),
                ClientIp = "127.0.0.1",
            });
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task SyncUniversitiesAndFacultiesAsync(
        AppDbContext db,
        IReadOnlyList<UniversitySeedItem> universities,
        CancellationToken ct)
    {
        var existingUniversities = ToSafeDictionary(
            await db.Universities.IgnoreQueryFilters().Include(u => u.Faculties).ToListAsync(ct),
            u => u.Name,
            u => u.DeletedAt is null);

        foreach (var uniDef in universities)
        {
            if (existingUniversities.TryGetValue(uniDef.Name, out var existingUni))
            {
                if (existingUni.DeletedAt is not null)
                    continue;

                existingUni.City = uniDef.City;
                existingUni.Country = uniDef.Country;
                existingUni.LogoUrl = uniDef.LogoUrl;
                existingUni.Website = uniDef.Website;
                foreach (var facDef in uniDef.Faculties)
                {
                    if (!existingUni.Faculties.Any(f => string.Equals(f.Code, facDef.Code, StringComparison.OrdinalIgnoreCase)))
                    {
                        existingUni.Faculties.Add(new Faculty { Name = facDef.Name, Code = facDef.Code });
                    }
                }
                continue;
            }

            var uni = new University
            {
                Name = uniDef.Name,
                City = uniDef.City,
                Country = uniDef.Country,
                Website = uniDef.Website,
                LogoUrl = uniDef.LogoUrl,
            };

            foreach (var facDef in uniDef.Faculties)
            {
                uni.Faculties.Add(new Faculty { Name = facDef.Name, Code = facDef.Code });
            }

            db.Universities.Add(uni);
            existingUniversities[uni.Name] = uni;
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SyncSkillsAndBadgesAsync(
        AppDbContext db,
        IReadOnlyList<SkillSeedItem> skills,
        IReadOnlyList<BadgeSeedItem> badges,
        CancellationToken ct)
    {
        var existingSkills = await db.Skills.IgnoreQueryFilters()
            .Select(s => s.Name)
            .ToListAsync(ct);
        var existingSkillSet = new HashSet<string>(existingSkills, StringComparer.OrdinalIgnoreCase);

        foreach (var s in skills)
        {
            if (existingSkillSet.Add(s.Name))
            {
                db.Skills.Add(new Skill
                {
                    Name = s.Name,
                    Category = s.Category,
                    Description = s.Description,
                });
            }
        }

        var existingBadges = await db.Badges.IgnoreQueryFilters()
            .Select(b => b.Name)
            .ToListAsync(ct);
        var existingBadgeSet = new HashSet<string>(existingBadges, StringComparer.OrdinalIgnoreCase);

        foreach (var b in badges)
        {
            if (existingBadgeSet.Add(b.Name) && Enum.TryParse<BadgeCriteriaType>(b.CriteriaType, true, out var criteriaType))
            {
                db.Badges.Add(new Badge
                {
                    Name = b.Name,
                    Description = b.Description,
                    IconUrl = b.IconUrl,
                    CriteriaType = criteriaType,
                    CriteriaValue = b.CriteriaValue,
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task<Dictionary<string, Student>> SyncStudentsAsync(
        AppDbContext db,
        IReadOnlyList<StudentSeedItem> seedStudents,
        string? devPasswordHash,
        bool resetExistingPasswords,
        CancellationToken ct)
    {
        var facultiesByCode = ToSafeDictionary(
            await db.Faculties.IgnoreQueryFilters()
                .Where(f => f.DeletedAt == null)
                .OrderBy(f => f.FacultyId)
                .ToListAsync(ct),
            f => f.Code,
            _ => true);

        var skillsByName = ToSafeDictionary(
            await db.Skills.IgnoreQueryFilters().Where(s => s.DeletedAt == null).ToListAsync(ct),
            s => s.Name,
            _ => true);

        var thresholdBadges = await db.Badges.IgnoreQueryFilters()
            .Where(b => b.DeletedAt == null && b.CriteriaType == BadgeCriteriaType.PointsThreshold)
            .ToListAsync(ct);

        var studentsByEmail = ToSafeDictionary(
            await db.Students.IgnoreQueryFilters().ToListAsync(ct),
            s => s.Email,
            s => s.DeletedAt is null);

        foreach (var def in seedStudents)
        {
            var affiliation = Enum.TryParse<AffiliationType>(def.AffiliationType, true, out var parsedAff)
                ? parsedAff
                : AffiliationType.UniversityStudent;

            int? facultyId = !string.IsNullOrWhiteSpace(def.FacultyCode) && facultiesByCode.TryGetValue(def.FacultyCode, out var fac)
                ? fac.FacultyId
                : null;

            if (!studentsByEmail.TryGetValue(def.Email, out var student))
            {
                student = new Student
                {
                    Email = def.Email.Trim().ToLowerInvariant(),
                    PasswordHash = devPasswordHash ?? CreateUnusablePasswordHash(),
                    FirstName = def.FirstName,
                    LastName = def.LastName,
                    AcademicYear = def.AcademicYear,
                    AffiliationType = affiliation,
                    City = def.City,
                    Role = UserRole.Student,
                    ShowEmailInDirectory = false,
                    FacultyId = facultyId,
                    SecurityStamp = Guid.NewGuid(),
                };
                db.Students.Add(student);
                studentsByEmail[student.Email] = student;
            }
            else if (student.DeletedAt is null)
            {
                if (resetExistingPasswords && devPasswordHash is not null)
                    student.PasswordHash = devPasswordHash;
                if (student.FacultyId != facultyId && facultyId is not null)
                    student.FacultyId = facultyId;
                student.AffiliationType = affiliation;
                if (string.IsNullOrWhiteSpace(student.City) && !string.IsNullOrWhiteSpace(def.City))
                    student.City = def.City;
            }
        }

        await db.SaveChangesAsync(ct);

        var seededStudentIds = studentsByEmail.Values
            .Where(s => s.DeletedAt is null)
            .Select(s => s.StudentId)
            .ToHashSet();

        var existingProfiles = new Dictionary<int, StudentProfile>();
        foreach (var profile in await db.StudentProfiles.IgnoreQueryFilters()
                     .Where(p => seededStudentIds.Contains(p.StudentId))
                     .OrderBy(p => p.DeletedAt == null ? 0 : 1)
                     .ToListAsync(ct))
        {
            existingProfiles.TryAdd(profile.StudentId, profile);
        }

        var existingInterests = (await db.StudentInterests.IgnoreQueryFilters()
                .Where(i => seededStudentIds.Contains(i.StudentId))
                .Select(i => new { i.StudentId, i.Interest })
                .ToListAsync(ct))
            .GroupBy(i => i.StudentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Interest).ToHashSet(StringComparer.OrdinalIgnoreCase));

        var existingSkillsByStudent = (await db.StudentSkills.IgnoreQueryFilters()
                .Where(ss => seededStudentIds.Contains(ss.StudentId))
                .Select(ss => new { ss.StudentId, ss.SkillId })
                .ToListAsync(ct))
            .GroupBy(ss => ss.StudentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.SkillId).ToHashSet());

        var studentsWithScores = (await db.ScoreTransactions
                .Where(t => seededStudentIds.Contains(t.StudentId))
                .Select(t => t.StudentId)
                .Distinct()
                .ToListAsync(ct))
            .ToHashSet();

        var existingBadgesByStudent = (await db.StudentBadges.IgnoreQueryFilters()
                .Where(sb => seededStudentIds.Contains(sb.StudentId))
                .Select(sb => new { sb.StudentId, sb.BadgeId })
                .ToListAsync(ct))
            .GroupBy(sb => sb.StudentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.BadgeId).ToHashSet());

        foreach (var def in seedStudents)
        {
            if (!studentsByEmail.TryGetValue(def.Email, out var student) || student.DeletedAt is not null)
                continue;

            if (!existingProfiles.TryGetValue(student.StudentId, out var profile))
            {
                db.StudentProfiles.Add(new StudentProfile
                {
                    StudentId = student.StudentId,
                    PhotoUrl = def.PhotoUrl,
                    Bio = def.Bio,
                    GitHubUrl = def.GitHubUrl,
                    LinkedInUrl = def.LinkedInUrl,
                });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(profile.PhotoUrl) && !string.IsNullOrWhiteSpace(def.PhotoUrl))
                    profile.PhotoUrl = def.PhotoUrl;
                if (string.IsNullOrWhiteSpace(profile.Bio) && !string.IsNullOrWhiteSpace(def.Bio))
                    profile.Bio = def.Bio;
            }

            if (!existingInterests.TryGetValue(student.StudentId, out var interestSet))
                interestSet = [];

            foreach (var interest in def.Interests)
            {
                if (!string.IsNullOrWhiteSpace(interest) && interestSet.Add(interest))
                {
                    db.StudentInterests.Add(new StudentInterest
                    {
                        StudentId = student.StudentId,
                        Interest = interest,
                    });
                }
            }

            if (!existingSkillsByStudent.TryGetValue(student.StudentId, out var skillSet))
                skillSet = [];

            foreach (var skDef in def.Skills)
            {
                if (skillsByName.TryGetValue(skDef.Name, out var skill) && skillSet.Add(skill.SkillId))
                {
                    var level = Enum.TryParse<ProficiencyLevel>(skDef.ProficiencyLevel, true, out var parsedLevel)
                        ? parsedLevel
                        : ProficiencyLevel.Intermediate;
                    db.StudentSkills.Add(new StudentSkill
                    {
                        StudentId = student.StudentId,
                        SkillId = skill.SkillId,
                        ProficiencyLevel = level,
                        YearsOfExperience = skDef.YearsOfExperience,
                    });
                }
            }

            if (def.Points > 0 && studentsWithScores.Add(student.StudentId))
            {
                db.ScoreTransactions.Add(new ScoreTransaction
                {
                    StudentId = student.StudentId,
                    Points = def.Points,
                    Reason = "Verified campus collaboration & workshop contributions",
                    SourceType = ScoreSourceType.Manual,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            if (!existingBadgesByStudent.TryGetValue(student.StudentId, out var badgeSet))
                badgeSet = [];

            foreach (var badge in thresholdBadges)
            {
                if (def.Points >= badge.CriteriaValue && badgeSet.Add(badge.BadgeId))
                {
                    db.StudentBadges.Add(new StudentBadge
                    {
                        StudentId = student.StudentId,
                        BadgeId = badge.BadgeId,
                        AwardedAt = DateTime.UtcNow,
                    });
                }
            }
        }

        await db.SaveChangesAsync(ct);
        return studentsByEmail;
    }

    private static async Task<Dictionary<string, Organization>> SyncOrganizationsAsync(
        AppDbContext db,
        IReadOnlyList<OrganizationSeedItem> orgDefs,
        Dictionary<string, University> unisByName,
        Dictionary<string, Student> studentsByEmail,
        CancellationToken ct)
    {
        var orgsByName = ToSafeDictionary(
            await db.Organizations.IgnoreQueryFilters().ToListAsync(ct),
            o => o.Name,
            o => o.DeletedAt is null);

        foreach (var spec in orgDefs)
        {
            var scope = Enum.TryParse<OrganizationScope>(spec.Scope, true, out var parsedScope)
                ? parsedScope
                : OrganizationScope.UniversityBranch;

            int? uniId = !string.IsNullOrWhiteSpace(spec.UniversityName) && unisByName.TryGetValue(spec.UniversityName, out var u)
                ? u.UniversityId
                : null;

            if (!orgsByName.TryGetValue(spec.Name, out var org))
            {
                org = new Organization
                {
                    Name = spec.Name,
                    Description = spec.Description,
                    Category = spec.Category,
                    LogoUrl = spec.LogoUrl,
                    Scope = scope,
                    UniversityId = uniId,
                    City = spec.City,
                    CoverImageUrl = spec.CoverImageUrl,
                    WebsiteUrl = spec.WebsiteUrl,
                    IsRecruiting = spec.IsRecruiting,
                    CurrentSeasonName = spec.CurrentSeasonName,
                };
                db.Organizations.Add(org);
                orgsByName[org.Name] = org;
            }
            else if (org.DeletedAt is null)
            {
                org.Description = spec.Description;
                org.Category = spec.Category;
                org.LogoUrl = spec.LogoUrl;
                org.Scope = scope;
                org.UniversityId = uniId;
                org.City = spec.City;
                org.WebsiteUrl = spec.WebsiteUrl;
                org.IsRecruiting = spec.IsRecruiting;
                org.CurrentSeasonName = spec.CurrentSeasonName;
            }
        }

        await db.SaveChangesAsync(ct);

        var orgIds = orgsByName.Values.Where(o => o.DeletedAt is null).Select(o => o.OrganizationId).ToHashSet();

        var existingUnits = new Dictionary<int, Dictionary<string, OrganizationUnit>>();
        foreach (var unit in await db.OrganizationUnits.IgnoreQueryFilters()
                     .Where(u => orgIds.Contains(u.OrganizationId))
                     .OrderBy(u => u.DeletedAt == null ? 0 : 1)
                     .ToListAsync(ct))
        {
            if (!existingUnits.TryGetValue(unit.OrganizationId, out var unitMap))
            {
                unitMap = new Dictionary<string, OrganizationUnit>(StringComparer.OrdinalIgnoreCase);
                existingUnits[unit.OrganizationId] = unitMap;
            }
            unitMap.TryAdd(unit.Code, unit);
        }

        foreach (var spec in orgDefs)
        {
            if (!orgsByName.TryGetValue(spec.Name, out var org) || org.DeletedAt is not null)
                continue;

            if (!existingUnits.TryGetValue(org.OrganizationId, out var unitMap))
            {
                unitMap = new Dictionary<string, OrganizationUnit>(StringComparer.OrdinalIgnoreCase);
                existingUnits[org.OrganizationId] = unitMap;
            }

            foreach (var uDef in spec.Units)
            {
                if (unitMap.ContainsKey(uDef.Code))
                    continue;

                var unitType = Enum.TryParse<OrganizationUnitType>(uDef.UnitType, true, out var parsedUnitType)
                    ? parsedUnitType
                    : OrganizationUnitType.TechnicalChapter;

                var unit = new OrganizationUnit
                {
                    OrganizationId = org.OrganizationId,
                    Name = uDef.Name,
                    Code = uDef.Code,
                    Description = uDef.Description,
                    UnitType = unitType,
                    IsRecruiting = uDef.IsRecruiting,
                    OpenSeats = uDef.OpenSeats,
                };
                db.OrganizationUnits.Add(unit);
                unitMap[uDef.Code] = unit;
            }
        }

        await db.SaveChangesAsync(ct);

        var existingMemberKeys = (await db.OrganizationMembers.IgnoreQueryFilters()
                .Where(m => orgIds.Contains(m.OrganizationId))
                .Select(m => new { m.OrganizationId, m.StudentId })
                .ToListAsync(ct))
            .Select(m => (m.OrganizationId, m.StudentId))
            .ToHashSet();

        foreach (var spec in orgDefs)
        {
            if (!orgsByName.TryGetValue(spec.Name, out var org) || org.DeletedAt is not null)
                continue;

            existingUnits.TryGetValue(org.OrganizationId, out var unitMap);

            foreach (var mDef in spec.Members)
            {
                if (!studentsByEmail.TryGetValue(mDef.StudentEmail, out var stu) || stu.DeletedAt is not null)
                    continue;

                if (!existingMemberKeys.Add((org.OrganizationId, stu.StudentId)))
                    continue;

                int? unitId = !string.IsNullOrWhiteSpace(mDef.UnitCode) && unitMap is not null && unitMap.TryGetValue(mDef.UnitCode, out var unit)
                    ? unit.OrganizationUnitId
                    : null;

                var tier = Enum.TryParse<OrganizationHierarchyTier>(mDef.HierarchyTier, true, out var parsedTier)
                    ? parsedTier
                    : OrganizationRoles.GetHierarchyTier(mDef.Role);

                db.OrganizationMembers.Add(new OrganizationMember
                {
                    OrganizationId = org.OrganizationId,
                    StudentId = stu.StudentId,
                    Role = mDef.Role,
                    OrganizationUnitId = unitId,
                    HierarchyTier = tier,
                    JoinedAt = DateTime.UtcNow.AddDays(-30),
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return orgsByName;
    }

    private static async Task SyncProjectsAsync(
        AppDbContext db,
        IReadOnlyList<ProjectSeedItem> projectDefs,
        Dictionary<string, Student> studentsByEmail,
        CancellationToken ct)
    {
        var skillsByName = ToSafeDictionary(
            await db.Skills.IgnoreQueryFilters().Where(s => s.DeletedAt == null).ToListAsync(ct),
            s => s.Name,
            _ => true);

        var existingProjects = ToSafeDictionary(
            await db.Projects.IgnoreQueryFilters().ToListAsync(ct),
            p => p.Title,
            p => p.DeletedAt is null);

        foreach (var pDef in projectDefs)
        {
            if (existingProjects.ContainsKey(pDef.Title))
                continue;

            if (!studentsByEmail.TryGetValue(pDef.CreatorEmail, out var creator) || creator.DeletedAt is not null)
                continue;

            var status = Enum.TryParse<ProjectStatus>(pDef.Status, true, out var parsedStatus)
                ? parsedStatus
                : ProjectStatus.Recruiting;

            var project = new Project
            {
                Title = pDef.Title,
                Description = pDef.Description,
                Status = status,
                MaxTeamSize = pDef.MaxTeamSize,
                CreatorId = creator.StudentId,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
            };
            db.Projects.Add(project);
            existingProjects[project.Title] = project;
        }

        await db.SaveChangesAsync(ct);

        var projectIds = existingProjects.Values.Where(p => p.DeletedAt is null).Select(p => p.ProjectId).ToHashSet();

        var existingReqSkills = (await db.ProjectRequiredSkills.IgnoreQueryFilters()
                .Where(rs => projectIds.Contains(rs.ProjectId))
                .Select(rs => new { rs.ProjectId, rs.SkillId })
                .ToListAsync(ct))
            .Select(x => (x.ProjectId, x.SkillId))
            .ToHashSet();

        var existingMembers = (await db.ProjectMembers.IgnoreQueryFilters()
                .Where(pm => projectIds.Contains(pm.ProjectId))
                .Select(pm => new { pm.ProjectId, pm.StudentId })
                .ToListAsync(ct))
            .GroupBy(x => x.ProjectId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.StudentId).ToHashSet());

        var existingTaskTitles = (await db.ProjectTasks.IgnoreQueryFilters()
                .Where(t => projectIds.Contains(t.ProjectId))
                .Select(t => new { t.ProjectId, t.Title })
                .ToListAsync(ct))
            .GroupBy(x => x.ProjectId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Title).ToHashSet(StringComparer.OrdinalIgnoreCase));

        var existingApps = (await db.ProjectApplications.IgnoreQueryFilters()
                .Where(a => projectIds.Contains(a.ProjectId))
                .Select(a => new { a.ProjectId, a.StudentId })
                .ToListAsync(ct))
            .Select(x => (x.ProjectId, x.StudentId))
            .ToHashSet();

        foreach (var pDef in projectDefs)
        {
            if (!existingProjects.TryGetValue(pDef.Title, out var project) || project.DeletedAt is not null)
                continue;

            foreach (var rs in pDef.RequiredSkills)
            {
                if (skillsByName.TryGetValue(rs.Name, out var skill) && existingReqSkills.Add((project.ProjectId, skill.SkillId)))
                {
                    var reqLevel = Enum.TryParse<ProficiencyLevel>(rs.RequiredLevel, true, out var parsedLevel)
                        ? parsedLevel
                        : ProficiencyLevel.Intermediate;
                    db.ProjectRequiredSkills.Add(new ProjectRequiredSkill
                    {
                        ProjectId = project.ProjectId,
                        SkillId = skill.SkillId,
                        RequiredLevel = reqLevel,
                        IsMandatory = rs.IsMandatory,
                    });
                }
            }

            if (!existingMembers.TryGetValue(project.ProjectId, out var memberSet))
            {
                memberSet = [];
                existingMembers[project.ProjectId] = memberSet;
            }

            foreach (var m in pDef.Members)
            {
                if (memberSet.Count >= project.MaxTeamSize)
                    break;
                if (studentsByEmail.TryGetValue(m.StudentEmail, out var stu) && stu.DeletedAt is null && memberSet.Add(stu.StudentId))
                {
                    db.ProjectMembers.Add(new ProjectMember
                    {
                        ProjectId = project.ProjectId,
                        StudentId = stu.StudentId,
                        Role = m.Role,
                        JoinedAt = DateTime.UtcNow.AddDays(-8),
                    });
                }
            }

            // Guarantee creator is always a team member
            if (memberSet.Count < project.MaxTeamSize && memberSet.Add(project.CreatorId))
            {
                db.ProjectMembers.Add(new ProjectMember
                {
                    ProjectId = project.ProjectId,
                    StudentId = project.CreatorId,
                    Role = "Leader",
                    JoinedAt = DateTime.UtcNow.AddDays(-10),
                });
            }

            if (!existingTaskTitles.TryGetValue(project.ProjectId, out var taskTitleSet))
                taskTitleSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var t in pDef.Tasks)
            {
                if (!taskTitleSet.Add(t.Title))
                    continue;

                int? assignedId = !string.IsNullOrWhiteSpace(t.AssignedStudentEmail)
                                  && studentsByEmail.TryGetValue(t.AssignedStudentEmail, out var assignee)
                                  && assignee.DeletedAt is null
                    ? assignee.StudentId
                    : null;

                var taskStatus = Enum.TryParse<ProjectTaskStatus>(t.Status, true, out var parsedTs)
                    ? parsedTs
                    : ProjectTaskStatus.Todo;
                var priority = Enum.TryParse<TaskPriority>(t.Priority, true, out var parsedPrio)
                    ? parsedPrio
                    : TaskPriority.Medium;

                db.ProjectTasks.Add(new ProjectTask
                {
                    ProjectId = project.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = taskStatus,
                    Priority = priority,
                    DueDate = DateTime.UtcNow.Date.AddDays(t.DueOffsetDays),
                    AssignedStudentId = assignedId,
                });
            }

            foreach (var app in pDef.Applications)
            {
                if (!studentsByEmail.TryGetValue(app.StudentEmail, out var applicant) || applicant.DeletedAt is not null)
                    continue;
                if (memberSet.Contains(applicant.StudentId))
                    continue;
                if (!existingApps.Add((project.ProjectId, applicant.StudentId)))
                    continue;

                var appStatus = Enum.TryParse<ApplicationStatus>(app.Status, true, out var parsedAs)
                    ? parsedAs
                    : ApplicationStatus.Pending;

                db.ProjectApplications.Add(new ProjectApplication
                {
                    ProjectId = project.ProjectId,
                    StudentId = applicant.StudentId,
                    Message = app.Message,
                    Status = appStatus,
                    AppliedAt = DateTime.UtcNow.AddDays(-2),
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SyncEventsAsync(
        AppDbContext db,
        IReadOnlyList<EventSeedItem> eventDefs,
        Dictionary<string, Organization> orgsByName,
        Dictionary<string, Student> studentsByEmail,
        CancellationToken ct)
    {
        var orgIds = orgsByName.Values.Where(o => o.DeletedAt is null).Select(o => o.OrganizationId).ToHashSet();

        var unitsByOrgCode = (await db.OrganizationUnits.IgnoreQueryFilters()
                .Where(u => orgIds.Contains(u.OrganizationId) && u.DeletedAt == null)
                .ToListAsync(ct))
            .GroupBy(u => u.OrganizationId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Code, x => x.OrganizationUnitId, StringComparer.OrdinalIgnoreCase));

        var existingEvents = new Dictionary<(int OrganizationId, string Title), Event>();
        foreach (var evt in await db.Events.IgnoreQueryFilters()
                     .Where(e => orgIds.Contains(e.OrganizationId))
                     .OrderBy(e => e.DeletedAt == null ? 0 : 1)
                     .ToListAsync(ct))
        {
            existingEvents.TryAdd((evt.OrganizationId, evt.Title), evt);
        }

        var seedEventsList = new List<(EventSeedItem Spec, Event Entity, DateTime StartTime)>();

        foreach (var evDef in eventDefs)
        {
            if (!orgsByName.TryGetValue(evDef.OrganizationName, out var org) || org.DeletedAt is not null)
                continue;

            int? unitId = !string.IsNullOrWhiteSpace(evDef.UnitCode)
                          && unitsByOrgCode.TryGetValue(org.OrganizationId, out var uMap)
                          && uMap.TryGetValue(evDef.UnitCode, out var foundUnitId)
                ? foundUnitId
                : null;

            var start = DateTime.UtcNow.Date.AddDays(evDef.StartOffsetDays).AddHours(evDef.StartHourUtc);

            if (!existingEvents.TryGetValue((org.OrganizationId, evDef.Title), out var evt))
            {
                var evType = Enum.TryParse<EventType>(evDef.Type, true, out var parsedType) ? parsedType : EventType.Workshop;
                var mode = Enum.TryParse<EventDeliveryMode>(evDef.DeliveryMode, true, out var parsedMode)
                    ? parsedMode
                    : EventDeliveryMode.InPerson;

                evt = new Event
                {
                    OrganizationId = org.OrganizationId,
                    OrganizationUnitId = unitId,
                    Title = evDef.Title,
                    Description = evDef.Description,
                    Type = evType,
                    DeliveryMode = mode,
                    IsPublic = evDef.IsPublic,
                    MeetingUrl = evDef.MeetingUrl,
                    Location = evDef.Location,
                    StartDate = start,
                    EndDate = start.AddHours(Math.Max(1, evDef.DurationHours)),
                    Capacity = Math.Max(1, evDef.Capacity),
                };
                db.Events.Add(evt);
                existingEvents[(org.OrganizationId, evDef.Title)] = evt;
            }
            else
            {
                start = evt.StartDate;
            }

            if (evt.DeletedAt is null)
                seedEventsList.Add((evDef, evt, start));
        }

        await db.SaveChangesAsync(ct);

        var eventIds = seedEventsList.Select(x => x.Entity.EventId).ToHashSet();

        var existingSessions = new Dictionary<(int EventId, int SessionNo), EventSession>();
        foreach (var session in await db.EventSessions.IgnoreQueryFilters()
                     .Where(s => eventIds.Contains(s.EventId))
                     .OrderBy(s => s.DeletedAt == null ? 0 : 1)
                     .ToListAsync(ct))
        {
            existingSessions.TryAdd((session.EventId, session.SessionNo), session);
        }

        var sessionPairs = new List<(EventSessionSeedItem Spec, EventSession Entity)>();

        foreach (var (evDef, evt, start) in seedEventsList)
        {
            foreach (var sDef in evDef.Sessions)
            {
                if (!existingSessions.TryGetValue((evt.EventId, sDef.SessionNo), out var session))
                {
                    var sessStart = start.AddHours(sDef.OffsetHours);
                    session = new EventSession
                    {
                        EventId = evt.EventId,
                        SessionNo = sDef.SessionNo,
                        Title = sDef.Title,
                        StartTime = sessStart,
                        EndTime = sessStart.AddHours(Math.Max(1, sDef.DurationHours)),
                        Room = sDef.Room,
                    };
                    db.EventSessions.Add(session);
                    existingSessions[(evt.EventId, sDef.SessionNo)] = session;
                }

                if (session.DeletedAt is null)
                    sessionPairs.Add((sDef, session));
            }
        }

        await db.SaveChangesAsync(ct);

        var existingRegs = (await db.EventRegistrations.IgnoreQueryFilters()
                .Where(r => eventIds.Contains(r.EventId))
                .Select(r => new { r.EventId, r.StudentId })
                .ToListAsync(ct))
            .GroupBy(r => r.EventId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.StudentId).ToHashSet());

        foreach (var (evDef, evt, _) in seedEventsList)
        {
            if (!existingRegs.TryGetValue(evt.EventId, out var regSet))
            {
                regSet = [];
                existingRegs[evt.EventId] = regSet;
            }

            foreach (var regDef in evDef.Registrations)
            {
                if (regSet.Count >= evt.Capacity)
                    break;
                if (!studentsByEmail.TryGetValue(regDef.StudentEmail, out var stu) || stu.DeletedAt is not null)
                    continue;
                if (!regSet.Add(stu.StudentId))
                    continue;

                var status = Enum.TryParse<RegistrationStatus>(regDef.Status, true, out var parsedStatus)
                    ? parsedStatus
                    : RegistrationStatus.Confirmed;

                db.EventRegistrations.Add(new EventRegistration
                {
                    EventId = evt.EventId,
                    StudentId = stu.StudentId,
                    Status = status,
                    RegisteredAt = DateTime.UtcNow.AddDays(-2),
                });
            }
        }

        var sessionIds = sessionPairs.Select(p => p.Entity.SessionId).ToHashSet();
        var existingAttendances = (await db.SessionAttendances.IgnoreQueryFilters()
                .Where(a => sessionIds.Contains(a.SessionId))
                .Select(a => new { a.SessionId, a.StudentId })
                .ToListAsync(ct))
            .Select(a => (a.SessionId, a.StudentId))
            .ToHashSet();

        foreach (var (sDef, session) in sessionPairs)
        {
            foreach (var email in sDef.AttendedEmails)
            {
                if (!studentsByEmail.TryGetValue(email, out var stu) || stu.DeletedAt is not null)
                    continue;
                if (!existingAttendances.Add((session.SessionId, stu.StudentId)))
                    continue;

                db.SessionAttendances.Add(new SessionAttendance
                {
                    SessionId = session.SessionId,
                    StudentId = stu.StudentId,
                    VerificationMethod = VerificationMethod.QRCode,
                    CheckInTime = DateTime.UtcNow.AddHours(-3),
                    RecordedByStudentId = stu.StudentId,
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SyncMembershipApplicationsAsync(
        AppDbContext db,
        IReadOnlyList<MembershipApplicationSeedItem> appDefs,
        Dictionary<string, Organization> orgsByName,
        Dictionary<string, Student> studentsByEmail,
        CancellationToken ct)
    {
        var orgIds = orgsByName.Values.Where(o => o.DeletedAt is null).Select(o => o.OrganizationId).ToHashSet();

        var unitsByOrgCode = (await db.OrganizationUnits.IgnoreQueryFilters()
                .Where(u => orgIds.Contains(u.OrganizationId) && u.DeletedAt == null)
                .ToListAsync(ct))
            .GroupBy(u => u.OrganizationId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Code, x => x.OrganizationUnitId, StringComparer.OrdinalIgnoreCase));

        var existingPendingPairs = (await db.MembershipApplications.IgnoreQueryFilters()
                .Where(a => orgIds.Contains(a.OrganizationId) && a.DeletedAt == null)
                .Select(a => new { a.OrganizationId, a.StudentId })
                .ToListAsync(ct))
            .Select(x => (x.OrganizationId, x.StudentId))
            .ToHashSet();

        var existingMemberPairs = (await db.OrganizationMembers.IgnoreQueryFilters()
                .Where(m => orgIds.Contains(m.OrganizationId) && m.DeletedAt == null)
                .Select(m => new { m.OrganizationId, m.StudentId })
                .ToListAsync(ct))
            .Select(x => (x.OrganizationId, x.StudentId))
            .ToHashSet();

        foreach (var appDef in appDefs)
        {
            if (!orgsByName.TryGetValue(appDef.OrganizationName, out var org) || org.DeletedAt is not null)
                continue;
            if (!studentsByEmail.TryGetValue(appDef.StudentEmail, out var stu) || stu.DeletedAt is not null)
                continue;
            if (!existingPendingPairs.Add((org.OrganizationId, stu.StudentId)))
                continue;

            var stage = Enum.TryParse<RecruitmentStage>(appDef.Stage, true, out var parsedStage)
                ? parsedStage
                : RecruitmentStage.Applied;

            if (stage != RecruitmentStage.Accepted && existingMemberPairs.Contains((org.OrganizationId, stu.StudentId)))
                continue;

            int? unitId = !string.IsNullOrWhiteSpace(appDef.UnitCode)
                          && unitsByOrgCode.TryGetValue(org.OrganizationId, out var uMap)
                          && uMap.TryGetValue(appDef.UnitCode, out var foundUnitId)
                ? foundUnitId
                : null;

            var status = stage switch
            {
                RecruitmentStage.Accepted => ApplicationStatus.Accepted,
                RecruitmentStage.Rejected => ApplicationStatus.Rejected,
                RecruitmentStage.Withdrawn => ApplicationStatus.Withdrawn,
                _ => ApplicationStatus.Pending,
            };

            db.MembershipApplications.Add(new MembershipApplication
            {
                OrganizationId = org.OrganizationId,
                StudentId = stu.StudentId,
                OrganizationUnitId = unitId,
                SeasonName = appDef.SeasonName ?? org.CurrentSeasonName,
                Message = appDef.Message,
                Status = status,
                Stage = stage,
                AppliedAt = DateTime.UtcNow.AddDays(-4),
                ReviewedAt = status == ApplicationStatus.Pending ? null : DateTime.UtcNow.AddDays(-1),
                EvaluationScore = appDef.EvaluationScore,
                InterviewNotes = appDef.InterviewNotes,
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static Dictionary<string, T> ToSafeDictionary<T>(
        IEnumerable<T> items,
        Func<T, string> keySelector,
        Func<T, bool> isActiveSelector)
    {
        var map = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items.OrderBy(x => isActiveSelector(x) ? 0 : 1))
        {
            var key = keySelector(item);
            if (!string.IsNullOrWhiteSpace(key))
                map.TryAdd(key, item);
        }
        return map;
    }

    private static string CreateUnusablePasswordHash() =>
        new PasswordHasher<Student>().HashPassword(
            null!, Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)));
}
