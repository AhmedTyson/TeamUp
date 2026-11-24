// Date: 2026-11-24
// Purpose: Organization service supporting Egyptian student-activity scopes, chapters/committees, 4-tier hierarchy, Kanban recruitment, participants, CSV export, and public preview (STRIDE THR-T-01, THR-R-01, THR-E-01).

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Domain.Events;

namespace TeamUp.Application.Services;

public sealed class OrganizationService(
    IAppDbContext db,
    IOrganizationAuthorizationService orgAuth,
    IAuditLogService auditLog) : IOrganizationService
{
    public async Task<PagedResult<OrganizationSummaryDto>> GetOrganizationsAsync(
        string? search,
        string? category,
        PageRequest page,
        OrganizationScope? scope = null,
        int? universityId = null,
        bool? recruitingOnly = null,
        CancellationToken ct = default)
    {
        var query = db.Organizations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term)
                || x.Description.Contains(term)
                || (x.City != null && x.City.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var value = category.Trim();
            query = query.Where(x => x.Category == value);
        }

        if (scope.HasValue)
            query = query.Where(x => x.Scope == scope.Value);

        if (universityId.HasValue)
            query = query.Where(x => x.UniversityId == universityId.Value);

        if (recruitingOnly == true)
            query = query.Where(x => x.IsRecruiting);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.IsRecruiting)
            .ThenBy(x => x.Name)
            .Skip(page.Skip)
            .Take(page.SafePageSize)
            .Select(x => new OrganizationSummaryDto(
                x.OrganizationId,
                x.Name,
                x.Description,
                x.Category,
                x.LogoUrl,
                db.OrganizationMembers.Count(m => m.OrganizationId == x.OrganizationId),
                x.Scope,
                x.UniversityId,
                x.University != null ? x.University.Name : null,
                x.City,
                x.CoverImageUrl,
                x.WebsiteUrl,
                x.IsRecruiting,
                x.CurrentSeasonName,
                db.OrganizationUnits.Count(u => u.OrganizationId == x.OrganizationId)))
            .ToListAsync(ct);

        return new PagedResult<OrganizationSummaryDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public Task<OrganizationDetailDto> GetOrganizationByIdAsync(int id, CancellationToken ct = default) =>
        LoadDetailCoreAsync(id, includeMemberEmails: true, ct);

    public Task<OrganizationDetailDto> GetPublicOrganizationByIdAsync(int id, CancellationToken ct = default) =>
        LoadDetailCoreAsync(id, includeMemberEmails: false, ct);

    private async Task<OrganizationDetailDto> LoadDetailCoreAsync(int id, bool includeMemberEmails, CancellationToken ct)
    {
        var organization = await db.Organizations.AsNoTracking()
            .Include(x => x.University)
            .FirstOrDefaultAsync(x => x.OrganizationId == id, ct)
            ?? throw new NotFoundException($"Organization with ID {id} was not found.");

        var members = await LoadMembersCoreAsync(id, includeMemberEmails, ct);
        var units = await GetUnitsAsync(id, ct);

        var eventsQuery = db.Events.AsNoTracking()
            .Where(x => x.OrganizationId == id);
        if (!includeMemberEmails)
            eventsQuery = eventsQuery.Where(x => x.IsPublic);

        var events = await eventsQuery
            .OrderBy(x => x.StartDate)
            .Select(x => new OrganizationEventSummaryDto(
                x.EventId, x.Title, x.Type, x.Location, x.StartDate, x.EndDate, x.DeliveryMode, x.IsPublic))
            .ToListAsync(ct);

        return new OrganizationDetailDto(
            organization.OrganizationId,
            organization.Name,
            organization.Description,
            organization.Category,
            organization.LogoUrl,
            members,
            events,
            organization.Scope,
            organization.UniversityId,
            organization.University?.Name,
            organization.City,
            organization.CoverImageUrl,
            organization.WebsiteUrl,
            organization.IsRecruiting,
            organization.CurrentSeasonName,
            units);
    }

    public async Task<OrganizationDetailDto> CreateOrganizationAsync(
        int creatorStudentId, CreateOrganizationRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (await db.Organizations.AnyAsync(x => x.Name == name, ct))
            throw new ConflictException($"An organization named '{name}' already exists.");

        if (request.UniversityId is > 0
            && !await db.Universities.AsNoTracking().AnyAsync(u => u.UniversityId == request.UniversityId.Value, ct))
        {
            throw new NotFoundException($"University with ID {request.UniversityId.Value} was not found.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var organization = new Organization
        {
            Name = name,
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            LogoUrl = NormalizeOptional(request.LogoUrl),
            Scope = request.Scope,
            UniversityId = request.UniversityId is > 0 ? request.UniversityId : null,
            City = NormalizeOptional(request.City),
            CoverImageUrl = NormalizeOptional(request.CoverImageUrl),
            WebsiteUrl = NormalizeOptional(request.WebsiteUrl),
            IsRecruiting = request.IsRecruiting,
            CurrentSeasonName = NormalizeOptional(request.CurrentSeasonName),
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync(ct);

        // Assign creator as HighBoard / Admin of THIS organization only (STRIDE THR-T-01 / THR-E-01)
        if (creatorStudentId > 0 && await db.Students.AnyAsync(s => s.StudentId == creatorStudentId, ct))
        {
            db.OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationId = organization.OrganizationId,
                StudentId = creatorStudentId,
                Role = OrganizationRoles.Admin,
                HierarchyTier = OrganizationHierarchyTier.HighBoard,
                JoinedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }

        await transaction.CommitAsync(ct);
        return await GetOrganizationByIdAsync(organization.OrganizationId, ct);
    }

    public async Task<OrganizationDetailDto> UpdateOrganizationAsync(
        int id, int actorStudentId, string? actorRole, UpdateOrganizationRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageOrganizationAsync(id, actorStudentId, actorRole, ct);

        var organization = await db.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == id, ct)
            ?? throw new NotFoundException($"Organization with ID {id} was not found.");

        var name = request.Name.Trim();
        if (!string.Equals(organization.Name, name, StringComparison.OrdinalIgnoreCase)
            && await db.Organizations.AnyAsync(x => x.OrganizationId != id && x.Name == name, ct))
            throw new ConflictException($"An organization named '{name}' already exists.");

        if (request.UniversityId is > 0
            && !await db.Universities.AsNoTracking().AnyAsync(u => u.UniversityId == request.UniversityId.Value, ct))
        {
            throw new NotFoundException($"University with ID {request.UniversityId.Value} was not found.");
        }

        organization.Name = name;
        organization.Description = request.Description.Trim();
        organization.Category = request.Category.Trim();
        organization.LogoUrl = NormalizeOptional(request.LogoUrl);
        organization.Scope = request.Scope;
        organization.UniversityId = request.UniversityId is > 0 ? request.UniversityId : null;
        organization.City = NormalizeOptional(request.City);
        organization.CoverImageUrl = NormalizeOptional(request.CoverImageUrl);
        organization.WebsiteUrl = NormalizeOptional(request.WebsiteUrl);
        organization.IsRecruiting = request.IsRecruiting;
        organization.CurrentSeasonName = NormalizeOptional(request.CurrentSeasonName);
        await db.SaveChangesAsync(ct);

        return await GetOrganizationByIdAsync(id, ct);
    }

    public async Task DeleteOrganizationAsync(int id, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var organization = await db.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == id, ct)
            ?? throw new NotFoundException($"Organization with ID {id} was not found.");
        var orgName = organization.Name;
        var members = await db.OrganizationMembers.Where(x => x.OrganizationId == id).ToListAsync(ct);
        var applications = await db.MembershipApplications.Where(x => x.OrganizationId == id).ToListAsync(ct);
        var units = await db.OrganizationUnits.Where(x => x.OrganizationId == id).ToListAsync(ct);
        var events = await db.Events.Include(x => x.Sessions)
            .Where(x => x.OrganizationId == id).ToListAsync(ct);

        db.OrganizationMembers.RemoveRange(members);
        db.MembershipApplications.RemoveRange(applications);
        foreach (var eventEntity in events)
            db.EventSessions.RemoveRange(eventEntity.Sessions);
        db.Events.RemoveRange(events);
        db.OrganizationUnits.RemoveRange(units);
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        await auditLog.RecordAsync(
            "EntitySoftDeleted",
            "Organization",
            id.ToString(),
            oldValues: new { organization.OrganizationId, Name = orgName },
            newValues: new { DeletedAt = organization.DeletedAt },
            ct: ct);
    }

    public Task<IReadOnlyList<OrganizationMemberDto>> GetMembersAsync(int organizationId, CancellationToken ct = default) =>
        LoadMembersCoreAsync(organizationId, includeEmail: true, ct);

    private async Task<IReadOnlyList<OrganizationMemberDto>> LoadMembersCoreAsync(
        int organizationId, bool includeEmail, CancellationToken ct)
    {
        if (!await db.Organizations.AnyAsync(x => x.OrganizationId == organizationId, ct))
            throw new NotFoundException($"Organization with ID {organizationId} was not found.");

        return await db.OrganizationMembers.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.HierarchyTier).ThenBy(x => x.Role).ThenBy(x => x.JoinedAt)
            .Select(x => new OrganizationMemberDto(
                x.StudentId,
                x.Student.FirstName,
                x.Student.LastName,
                includeEmail ? x.Student.Email : string.Empty,
                x.Role,
                x.JoinedAt,
                x.OrganizationUnitId,
                x.Unit != null ? x.Unit.Name : null,
                x.Unit != null ? x.Unit.Code : null,
                x.HierarchyTier))
            .ToListAsync(ct);
    }

    public async Task<OrganizationMemberDto> UpdateMemberRoleAsync(
        int organizationId, int studentId, int actorStudentId, string? actorRole,
        UpdateOrganizationMemberRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageOrganizationAsync(organizationId, actorStudentId, actorRole, ct);

        var member = await db.OrganizationMembers
            .Include(x => x.Student)
            .Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.StudentId == studentId, ct)
            ?? throw new NotFoundException("The requested organization member was not found.");

        OrganizationUnit? targetUnit = member.Unit;
        if (request.OrganizationUnitId.HasValue)
        {
            if (request.OrganizationUnitId.Value <= 0)
            {
                member.OrganizationUnitId = null;
                targetUnit = null;
            }
            else
            {
                targetUnit = await db.OrganizationUnits.FirstOrDefaultAsync(
                    u => u.OrganizationId == organizationId && u.OrganizationUnitId == request.OrganizationUnitId.Value, ct)
                    ?? throw new NotFoundException($"Organization unit #{request.OrganizationUnitId.Value} was not found.");
                member.OrganizationUnitId = targetUnit.OrganizationUnitId;
            }
        }

        var oldRole = member.Role;
        var nextRole = request.Role.Trim();
        member.Role = nextRole;
        member.HierarchyTier = request.HierarchyTier ?? OrganizationRoles.GetHierarchyTier(nextRole);
        await db.SaveChangesAsync(ct);

        await auditLog.RecordAsync(
            "OrganizationMemberRoleChanged",
            "OrganizationMember",
            $"{organizationId}:{studentId}",
            oldValues: new { Role = oldRole },
            newValues: new { member.Role, member.OrganizationUnitId, member.HierarchyTier },
            actorStudentIdOverride: actorStudentId > 0 ? actorStudentId : null,
            ct: ct);

        return new OrganizationMemberDto(
            member.StudentId,
            member.Student.FirstName,
            member.Student.LastName,
            member.Student.Email,
            member.Role,
            member.JoinedAt,
            member.OrganizationUnitId,
            targetUnit?.Name,
            targetUnit?.Code,
            member.HierarchyTier);
    }

    public async Task RemoveMemberAsync(
        int organizationId, int studentId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        if (actorStudentId != studentId)
            await orgAuth.EnsureCanManageOrganizationAsync(organizationId, actorStudentId, actorRole, ct);

        var member = await db.OrganizationMembers.FirstOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.StudentId == studentId, ct)
            ?? throw new NotFoundException("The requested organization member was not found.");

        db.OrganizationMembers.Remove(member);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<OrganizationUnitDto>> GetUnitsAsync(int organizationId, CancellationToken ct = default)
    {
        if (!await db.Organizations.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId, ct))
            throw new NotFoundException($"Organization with ID {organizationId} was not found.");

        return await db.OrganizationUnits.AsNoTracking()
            .Where(u => u.OrganizationId == organizationId)
            .OrderBy(u => u.UnitType).ThenBy(u => u.Name)
            .Select(u => new OrganizationUnitDto(
                u.OrganizationUnitId,
                u.OrganizationId,
                u.Name,
                u.Code,
                u.Description,
                u.UnitType,
                u.IsRecruiting,
                u.OpenSeats,
                db.OrganizationMembers.Count(m => m.OrganizationId == organizationId && m.OrganizationUnitId == u.OrganizationUnitId)))
            .ToListAsync(ct);
    }

    public async Task<OrganizationUnitDto> CreateUnitAsync(
        int organizationId, int actorStudentId, string? actorRole, CreateOrganizationUnitRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageOrganizationAsync(organizationId, actorStudentId, actorRole, ct);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.OrganizationUnits.AnyAsync(u => u.OrganizationId == organizationId && u.Code == code, ct))
            throw new ConflictException($"A unit with code '{code}' already exists in this organization.");

        var unit = new OrganizationUnit
        {
            OrganizationId = organizationId,
            Name = request.Name.Trim(),
            Code = code,
            Description = NormalizeOptional(request.Description),
            UnitType = request.UnitType,
            IsRecruiting = request.IsRecruiting,
            OpenSeats = request.OpenSeats
        };
        db.OrganizationUnits.Add(unit);
        await db.SaveChangesAsync(ct);

        return new OrganizationUnitDto(
            unit.OrganizationUnitId, unit.OrganizationId, unit.Name, unit.Code,
            unit.Description, unit.UnitType, unit.IsRecruiting, unit.OpenSeats, 0);
    }

    public async Task<OrganizationUnitDto> UpdateUnitAsync(
        int organizationId, int unitId, int actorStudentId, string? actorRole, UpdateOrganizationUnitRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageOrganizationAsync(organizationId, actorStudentId, actorRole, ct);

        var unit = await db.OrganizationUnits.FirstOrDefaultAsync(
            u => u.OrganizationId == organizationId && u.OrganizationUnitId == unitId, ct)
            ?? throw new NotFoundException($"Organization unit #{unitId} was not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (!string.Equals(unit.Code, code, StringComparison.OrdinalIgnoreCase)
            && await db.OrganizationUnits.AnyAsync(u => u.OrganizationId == organizationId && u.OrganizationUnitId != unitId && u.Code == code, ct))
        {
            throw new ConflictException($"A unit with code '{code}' already exists in this organization.");
        }

        unit.Name = request.Name.Trim();
        unit.Code = code;
        unit.Description = NormalizeOptional(request.Description);
        unit.UnitType = request.UnitType;
        unit.IsRecruiting = request.IsRecruiting;
        unit.OpenSeats = request.OpenSeats;
        await db.SaveChangesAsync(ct);

        var memberCount = await db.OrganizationMembers.CountAsync(
            m => m.OrganizationId == organizationId && m.OrganizationUnitId == unitId, ct);

        return new OrganizationUnitDto(
            unit.OrganizationUnitId, unit.OrganizationId, unit.Name, unit.Code,
            unit.Description, unit.UnitType, unit.IsRecruiting, unit.OpenSeats, memberCount);
    }

    public async Task DeleteUnitAsync(
        int organizationId, int unitId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageOrganizationAsync(organizationId, actorStudentId, actorRole, ct);

        var unit = await db.OrganizationUnits.FirstOrDefaultAsync(
            u => u.OrganizationId == organizationId && u.OrganizationUnitId == unitId, ct)
            ?? throw new NotFoundException($"Organization unit #{unitId} was not found.");

        var members = await db.OrganizationMembers
            .Where(m => m.OrganizationId == organizationId && m.OrganizationUnitId == unitId)
            .ToListAsync(ct);
        foreach (var member in members)
            member.OrganizationUnitId = null;

        db.OrganizationUnits.Remove(unit);
        await db.SaveChangesAsync(ct);
    }

    public async Task<OrganizationHierarchyDto> GetHierarchyAsync(
        int organizationId, bool includeEmails = true, CancellationToken ct = default)
    {
        var org = await db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrganizationId == organizationId, ct)
            ?? throw new NotFoundException($"Organization with ID {organizationId} was not found.");

        var members = await LoadMembersCoreAsync(organizationId, includeEmails, ct);
        var units = await GetUnitsAsync(organizationId, ct);
        var participantsCount = await db.EventRegistrations.AsNoTracking()
            .Where(r => r.Event.OrganizationId == organizationId && r.Status == RegistrationStatus.Confirmed)
            .Select(r => r.StudentId)
            .Distinct()
            .CountAsync(ct);

        var highBoard = members
            .Where(m => m.HierarchyTier == OrganizationHierarchyTier.HighBoard || OrganizationRoles.IsAdminRole(m.Role))
            .ToList();
        var committeeLeadership = members
            .Where(m => !highBoard.Contains(m)
                && (m.HierarchyTier == OrganizationHierarchyTier.CommitteeLeadership || OrganizationRoles.IsCommitteeLeaderRole(m.Role)))
            .ToList();
        var organizingCrew = members
            .Where(m => !highBoard.Contains(m) && !committeeLeadership.Contains(m))
            .ToList();

        return new OrganizationHierarchyDto(
            org.OrganizationId,
            org.Name,
            org.Scope,
            highBoard,
            committeeLeadership,
            organizingCrew,
            units,
            participantsCount);
    }

    public async Task<IReadOnlyList<OrganizationParticipantDto>> GetParticipantsAsync(
        int organizationId, CancellationToken ct = default)
    {
        if (!await db.Organizations.AsNoTracking().AnyAsync(o => o.OrganizationId == organizationId, ct))
            throw new NotFoundException($"Organization with ID {organizationId} was not found.");

        var registrations = await db.EventRegistrations.AsNoTracking()
            .Where(r => r.Event.OrganizationId == organizationId && r.Status == RegistrationStatus.Confirmed)
            .Select(r => new
            {
                r.StudentId,
                r.Student.FirstName,
                r.Student.LastName,
                r.Student.AffiliationType,
                r.Student.City,
                FacultyName = r.Student.Faculty != null ? r.Student.Faculty.Name : null,
                UniversityName = r.Student.Faculty != null ? r.Student.Faculty.University.Name : null,
                EventTitle = r.Event.Title,
                r.RegisteredAt
            })
            .ToListAsync(ct);

        if (registrations.Count == 0)
            return [];

        var attendancesByStudent = await db.SessionAttendances.AsNoTracking()
            .Where(a => a.Session.Event.OrganizationId == organizationId)
            .GroupBy(a => a.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count(), LatestCheckIn = g.Max(x => x.CheckInTime) })
            .ToDictionaryAsync(x => x.StudentId, ct);

        return registrations
            .GroupBy(r => r.StudentId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(x => x.RegisteredAt).First();
                attendancesByStudent.TryGetValue(g.Key, out var att);
                var affiliationLabel = latest.UniversityName is not null && latest.FacultyName is not null
                    ? $"{latest.FacultyName} · {latest.UniversityName}"
                    : AffiliationFormatting.FormatAffiliationLabel(latest.AffiliationType)
                      + (string.IsNullOrWhiteSpace(latest.City) ? "" : $" · {latest.City}");

                var lastActive = att is not null && att.LatestCheckIn > latest.RegisteredAt
                    ? att.LatestCheckIn
                    : latest.RegisteredAt;

                return new OrganizationParticipantDto(
                    g.Key,
                    latest.FirstName,
                    latest.LastName,
                    affiliationLabel,
                    g.Count(),
                    att?.Count ?? 0,
                    latest.EventTitle,
                    lastActive);
            })
            .OrderByDescending(p => p.SessionsAttendedCount)
            .ThenByDescending(p => p.EventsRegisteredCount)
            .ThenBy(p => p.FirstName)
            .ToList();
    }

    public async Task<IReadOnlyList<MembershipApplicationDto>> GetMyApplicationsAsync(
        int studentId, CancellationToken ct = default)
    {
        return await db.MembershipApplications.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.AppliedAt)
            .Select(x => new MembershipApplicationDto(
                x.ApplicationId,
                x.OrganizationId,
                x.StudentId,
                x.Student.FirstName,
                x.Student.LastName,
                x.Student.Email,
                x.Message,
                x.Status,
                x.AppliedAt,
                x.ReviewedAt,
                x.OrganizationUnitId,
                x.Unit != null ? x.Unit.Name : null,
                x.Unit != null ? x.Unit.Code : null,
                x.SeasonName,
                x.Stage,
                x.EvaluationScore,
                x.InterviewNotes,
                x.Organization.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MembershipApplicationDto>> GetApplicationsAsync(
        int organizationId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        var (isFullOrgManager, isHrReviewer, managedUnitId) = await orgAuth.EnsureCanReviewRecruitmentAsync(
            organizationId, actorStudentId, actorRole, targetUnitId: null, ct);

        var query = db.MembershipApplications.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);

        if (!isFullOrgManager && !isHrReviewer && managedUnitId.HasValue)
            query = query.Where(x => x.OrganizationUnitId == managedUnitId.Value);

        return await query
            .OrderByDescending(x => x.AppliedAt)
            .Select(x => new MembershipApplicationDto(
                x.ApplicationId,
                x.OrganizationId,
                x.StudentId,
                x.Student.FirstName,
                x.Student.LastName,
                x.Student.Email,
                x.Message,
                x.Status,
                x.AppliedAt,
                x.ReviewedAt,
                x.OrganizationUnitId,
                x.Unit != null ? x.Unit.Name : null,
                x.Unit != null ? x.Unit.Code : null,
                x.SeasonName,
                x.Stage,
                x.EvaluationScore,
                x.InterviewNotes,
                x.Organization.Name))
            .ToListAsync(ct);
    }

    public async Task<MembershipApplicationDto> ApplyForMembershipAsync(
        int organizationId, int studentId, CreateMembershipApplicationRequest request, CancellationToken ct = default)
    {
        var organization = await db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, ct)
            ?? throw new NotFoundException($"Organization with ID {organizationId} was not found.");
        if (!await db.Students.AnyAsync(x => x.StudentId == studentId, ct))
            throw new NotFoundException($"Student with ID {studentId} was not found.");
        if (await db.OrganizationMembers.AnyAsync(x => x.OrganizationId == organizationId && x.StudentId == studentId, ct))
            throw new ConflictException("You are already a member of this organization.");
        if (await db.MembershipApplications.AnyAsync(x => x.OrganizationId == organizationId
            && x.StudentId == studentId && x.Status == ApplicationStatus.Pending, ct))
            throw new ConflictException("You already have a pending application for this organization.");

        if (request.OrganizationUnitId is > 0 && !await db.OrganizationUnits.AnyAsync(
            u => u.OrganizationId == organizationId && u.OrganizationUnitId == request.OrganizationUnitId.Value, ct))
        {
            throw new NotFoundException($"Organization unit #{request.OrganizationUnitId.Value} was not found.");
        }

        var application = new MembershipApplication
        {
            OrganizationId = organizationId,
            StudentId = studentId,
            Message = NormalizeOptional(request.Message),
            OrganizationUnitId = request.OrganizationUnitId is > 0 ? request.OrganizationUnitId : null,
            SeasonName = NormalizeOptional(request.SeasonName) ?? organization.CurrentSeasonName,
            Status = ApplicationStatus.Pending,
            Stage = RecruitmentStage.Applied,
            AppliedAt = DateTime.UtcNow
        };
        db.MembershipApplications.Add(application);
        await db.SaveChangesAsync(ct);
        return await GetApplicationDtoAsync(application.ApplicationId, ct);
    }

    public async Task<MembershipApplicationDto> ReviewApplicationAsync(
        int organizationId, int applicationId, int actorStudentId, string? actorRole,
        ReviewMembershipApplicationRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageOrganizationAsync(organizationId, actorStudentId, actorRole, ct);

        if (request.Status is not (ApplicationStatus.Accepted or ApplicationStatus.Rejected))
            throw new BadRequestException("An application can only be accepted or rejected by an organization admin.");

        var nextStage = request.Status == ApplicationStatus.Accepted
            ? RecruitmentStage.Accepted
            : RecruitmentStage.Rejected;

        return await TransitionApplicationStageCoreAsync(
            organizationId,
            applicationId,
            actorStudentId,
            new UpdateApplicationStageRequest(nextStage),
            ct);
    }

    public async Task<MembershipApplicationDto> UpdateApplicationStageAsync(
        int organizationId, int applicationId, int actorStudentId, string? actorRole,
        UpdateApplicationStageRequest request, CancellationToken ct = default)
    {
        var existingUnitId = await db.MembershipApplications.AsNoTracking()
            .Where(a => a.OrganizationId == organizationId && a.ApplicationId == applicationId)
            .Select(a => a.OrganizationUnitId)
            .FirstOrDefaultAsync(ct);

        await orgAuth.EnsureCanReviewRecruitmentAsync(
            organizationId, actorStudentId, actorRole, request.OrganizationUnitId ?? existingUnitId, ct);

        return await TransitionApplicationStageCoreAsync(
            organizationId,
            applicationId,
            actorStudentId,
            request,
            ct);
    }

    private async Task<MembershipApplicationDto> TransitionApplicationStageCoreAsync(
        int organizationId,
        int applicationId,
        int actorStudentId,
        UpdateApplicationStageRequest request,
        CancellationToken ct)
    {
        if (request.Stage == RecruitmentStage.Withdrawn)
            throw new BadRequestException("Reviewers cannot set an application stage to Withdrawn.");

        if (request.OrganizationUnitId is > 0 && !await db.OrganizationUnits.AnyAsync(
            u => u.OrganizationId == organizationId && u.OrganizationUnitId == request.OrganizationUnitId.Value, ct))
        {
            throw new NotFoundException($"Organization unit #{request.OrganizationUnitId.Value} was not found.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var application = await db.MembershipApplications
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.ApplicationId == applicationId, ct)
            ?? throw new NotFoundException("Membership application was not found.");

        if (application.Status != ApplicationStatus.Pending)
            throw new ConflictException("Only pending applications can be updated.");

        if (request.OrganizationUnitId.HasValue)
            application.OrganizationUnitId = request.OrganizationUnitId.Value > 0 ? request.OrganizationUnitId.Value : null;
        if (request.EvaluationScore.HasValue)
            application.EvaluationScore = request.EvaluationScore.Value;
        if (request.InterviewNotes is not null)
            application.InterviewNotes = NormalizeOptional(request.InterviewNotes);

        application.ReviewerStudentId = actorStudentId > 0 ? actorStudentId : null;
        application.Stage = request.Stage;

        if (request.Stage is RecruitmentStage.Accepted or RecruitmentStage.Rejected)
        {
            var decisionStatus = request.Stage == RecruitmentStage.Accepted
                ? ApplicationStatus.Accepted
                : ApplicationStatus.Rejected;

            if (decisionStatus == ApplicationStatus.Accepted)
            {
                var previousMembership = await db.OrganizationMembers.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.StudentId == application.StudentId, ct);
                if (previousMembership is null)
                {
                    db.OrganizationMembers.Add(new OrganizationMember
                    {
                        OrganizationId = organizationId,
                        StudentId = application.StudentId,
                        Role = OrganizationRoles.Member,
                        OrganizationUnitId = application.OrganizationUnitId,
                        HierarchyTier = OrganizationHierarchyTier.OrganizingCrew,
                        JoinedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    previousMembership.Role = OrganizationRoles.Member;
                    previousMembership.OrganizationUnitId = application.OrganizationUnitId;
                    previousMembership.HierarchyTier = OrganizationHierarchyTier.OrganizingCrew;
                    previousMembership.JoinedAt = DateTime.UtcNow;
                    previousMembership.DeletedAt = null;
                }
            }

            application.Status = decisionStatus;
            application.ReviewedAt = DateTime.UtcNow;
            application.AddDomainEvent(new ApplicationReviewedDomainEvent(
                application.StudentId,
                application.Organization.Name,
                "Organization",
                decisionStatus));
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return await GetApplicationDtoAsync(application.ApplicationId, ct);
    }

    public async Task WithdrawApplicationAsync(int organizationId, int applicationId, int studentId, CancellationToken ct = default)
    {
        var application = await db.MembershipApplications
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.ApplicationId == applicationId, ct)
            ?? throw new NotFoundException("Membership application was not found.");
        if (application.StudentId != studentId)
            throw new ForbiddenException("You are not allowed to withdraw this application.");
        if (application.Status != ApplicationStatus.Pending)
            throw new ConflictException("Only pending applications can be withdrawn.");

        application.Status = ApplicationStatus.Withdrawn;
        application.Stage = RecruitmentStage.Withdrawn;
        application.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<string> ExportApplicationsCsvAsync(
        int organizationId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        var apps = await GetApplicationsAsync(organizationId, actorStudentId, actorRole, ct);
        var headers = new[]
        {
            "ApplicationId", "StudentId", "FullName", "Email", "Unit", "Season",
            "Stage", "Status", "EvaluationScore", "AppliedAtUtc", "ReviewedAtUtc", "Message", "InterviewNotes"
        };

        var rows = apps.Select(a => new[]
        {
            a.ApplicationId.ToString(),
            a.StudentId.ToString(),
            $"{a.StudentFirstName} {a.StudentLastName}",
            a.StudentEmail,
            a.UnitName ?? a.UnitCode ?? "General",
            a.SeasonName ?? string.Empty,
            a.Stage.ToString(),
            a.Status.ToString(),
            a.EvaluationScore?.ToString("0.##") ?? string.Empty,
            a.AppliedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            a.ReviewedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
            a.Message ?? string.Empty,
            a.InterviewNotes ?? string.Empty
        });

        return CsvExportHelper.BuildCsv(headers, rows);
    }

    private async Task<MembershipApplicationDto> GetApplicationDtoAsync(int applicationId, CancellationToken ct)
    {
        var app = await db.MembershipApplications.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Organization)
            .Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId, ct)
            ?? throw new NotFoundException("Membership application was not found.");

        return new MembershipApplicationDto(
            app.ApplicationId,
            app.OrganizationId,
            app.StudentId,
            app.Student.FirstName,
            app.Student.LastName,
            app.Student.Email,
            app.Message,
            app.Status,
            app.AppliedAt,
            app.ReviewedAt,
            app.OrganizationUnitId,
            app.Unit?.Name,
            app.Unit?.Code,
            app.SeasonName,
            app.Stage,
            app.EvaluationScore,
            app.InterviewNotes,
            app.Organization.Name);
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
