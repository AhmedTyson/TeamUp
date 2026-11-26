// Date: 2026-11-24
// Purpose: Organization service supporting Egyptian student-activity scopes, chapters/committees, 4-tier hierarchy, Kanban recruitment, participants, CSV export, and public preview (STRIDE THR-T-01, THR-R-01, THR-E-01).

using System.Data;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Images;
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
    ISecurityStampValidator stampValidator,
    IAuditLogService auditLog,
    TimeProvider? timeProvider = null) : IOrganizationService
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    #region Discovery & Organization CRUD

    public async Task<IReadOnlyList<string>> GetOrganizationCategoriesAsync(CancellationToken ct = default)
    {
        // Distinct categories actually in use, most used first. Categories are free text, so the set can only
        // come from the data; this feeds the directory filter chips instead of a compiled-in list.
        return await db.Organizations.AsNoTracking()
            .Where(x => x.Category != null && x.Category != string.Empty)
            .GroupBy(x => x.Category)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => g.Key)
            .ToListAsync(ct);
    }

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
        items = items
            .Select(i => i with { CoverImageUrl = i.CoverImageUrl ?? DefaultCoverCatalog.UrlFor($"org:{i.OrganizationId}") })
            .ToList();

        return new PagedResult<OrganizationSummaryDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public Task<OrganizationDetailDto> GetOrganizationByIdAsync(
        int id, int viewerStudentId = 0, string? viewerRole = null, CancellationToken ct = default) =>
        LoadDetailCoreAsync(id, includeMemberEmails: true, viewerStudentId, viewerRole, ct);

    public Task<OrganizationDetailDto> GetPublicOrganizationByIdAsync(int id, CancellationToken ct = default) =>
        LoadDetailCoreAsync(id, includeMemberEmails: false, 0, null, ct);

    private async Task<OrganizationDetailDto> LoadDetailCoreAsync(
        int id, bool includeMemberEmails, int viewerStudentId, string? viewerRole, CancellationToken ct)
    {
        var organization = await db.Organizations.AsNoTracking()
            .Include(x => x.University)
            .FirstOrDefaultAsync(x => x.OrganizationId == id, ct)
            ?? throw new NotFoundException($"Organization with ID {id} was not found.");

        var members = await LoadMembersCoreAsync(id, includeMemberEmails, viewerStudentId, viewerRole, ct);
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
            organization.CoverImageUrl ?? DefaultCoverCatalog.UrlFor($"org:{organization.OrganizationId}"),
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

        var organizationId = await db.ExecuteInTransactionAsync(IsolationLevel.ReadCommitted, async token =>
        {
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
            await db.SaveChangesAsync(token);

            // Assign creator as HighBoard / Admin of THIS organization only (STRIDE THR-T-01 / THR-E-01)
            if (creatorStudentId > 0 && await db.Students.AnyAsync(s => s.StudentId == creatorStudentId, token))
            {
                db.OrganizationMembers.Add(new OrganizationMember
                {
                    OrganizationId = organization.OrganizationId,
                    StudentId = creatorStudentId,
                    Role = OrganizationRoles.Admin,
                    HierarchyTier = OrganizationHierarchyTier.HighBoard,
                    JoinedAt = _time.GetUtcNow().UtcDateTime
                });
                await db.SaveChangesAsync(token);
            }

            return organization.OrganizationId;
        }, ct);

        return await GetOrganizationByIdAsync(organizationId, creatorStudentId, null, ct);
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

        return await GetOrganizationByIdAsync(id, actorStudentId, actorRole, ct);
    }

    public async Task DeleteOrganizationAsync(int id, CancellationToken ct = default)
    {
        var (orgName, deletedAt) = await db.ExecuteInTransactionAsync(IsolationLevel.ReadCommitted, async token =>
        {
            var organization = await db.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == id, token)
                ?? throw new NotFoundException($"Organization with ID {id} was not found.");
            var name = organization.Name;
            var members = await db.OrganizationMembers.Where(x => x.OrganizationId == id).ToListAsync(token);
            var applications = await db.MembershipApplications.Where(x => x.OrganizationId == id).ToListAsync(token);
            var units = await db.OrganizationUnits.Where(x => x.OrganizationId == id).ToListAsync(token);
            var events = await db.Events.Include(x => x.Sessions)
                .Where(x => x.OrganizationId == id).ToListAsync(token);

            db.OrganizationMembers.RemoveRange(members);
            db.MembershipApplications.RemoveRange(applications);
            foreach (var eventEntity in events)
                db.EventSessions.RemoveRange(eventEntity.Sessions);
            db.Events.RemoveRange(events);
            db.OrganizationUnits.RemoveRange(units);
            db.Organizations.Remove(organization);
            await db.SaveChangesAsync(token);
            return (name, organization.DeletedAt);
        }, ct);

        await auditLog.RecordAsync(
            "EntitySoftDeleted",
            "Organization",
            id.ToString(),
            oldValues: new { OrganizationId = id, Name = orgName },
            newValues: new { DeletedAt = deletedAt },
            ct: ct);
    }

    #endregion

    #region Members

    public Task<IReadOnlyList<OrganizationMemberDto>> GetMembersAsync(
        int organizationId, int viewerStudentId = 0, string? viewerRole = null, CancellationToken ct = default) =>
        LoadMembersCoreAsync(organizationId, includeEmail: true, viewerStudentId, viewerRole, ct);

    private async Task<IReadOnlyList<OrganizationMemberDto>> LoadMembersCoreAsync(
        int organizationId, bool includeEmail, int viewerStudentId, string? viewerRole, CancellationToken ct)
    {
        if (!await db.Organizations.AnyAsync(x => x.OrganizationId == organizationId, ct))
            throw new NotFoundException($"Organization with ID {organizationId} was not found.");

        // Confidentiality: member emails are personal data, not org metadata. Only org managers (who need them to run
        // the org), the member themselves, and members who opted in via ShowEmailInDirectory are visible. Before this,
        // any authenticated student could harvest every member's email through the detail/members/hierarchy endpoints,
        // which made the directory opt-in meaningless (THR-I-01).
        var viewerIsManager = includeEmail && viewerStudentId > 0
            && (string.Equals(viewerRole, nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase)
                || await orgAuth.IsOrgAdminAsync(organizationId, viewerStudentId, ct));

        return await db.OrganizationMembers.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.HierarchyTier).ThenBy(x => x.Role).ThenBy(x => x.JoinedAt)
            .Select(x => new OrganizationMemberDto(
                x.StudentId,
                x.Student.FirstName,
                x.Student.LastName,
                includeEmail && (viewerIsManager || x.StudentId == viewerStudentId || x.Student.ShowEmailInDirectory)
                    ? x.Student.Email
                    : string.Empty,
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
        // Business rule: demoting the last administrator out of every admin role orphans the org.
        if (!OrganizationRoles.IsAdminRole(nextRole))
            await EnsureNotLastAdminAsync(organizationId, member, ct);
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

        // Business rule: an org can never lose its last administrator, otherwise nobody can
        // manage it (events, members, recruitment) and it is orphaned until a PlatformAdmin steps in.
        await EnsureNotLastAdminAsync(organizationId, member, ct);

        db.OrganizationMembers.Remove(member);
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureNotLastAdminAsync(int organizationId, OrganizationMember target, CancellationToken ct)
    {
        if (!OrganizationRoles.IsAdminRole(target.Role) && target.HierarchyTier != OrganizationHierarchyTier.HighBoard)
            return;

        var hasOtherAdmin = await db.OrganizationMembers.AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == organizationId
                    && x.StudentId != target.StudentId
                    && (x.HierarchyTier == OrganizationHierarchyTier.HighBoard
                        || OrganizationRoles.AdministrativeRoleNames.Contains(x.Role)),
                ct);

        if (!hasOtherAdmin)
            throw new ConflictException(
                "This member is the organization's last administrator. Promote another member to an admin role first.");
    }

    #endregion

    #region Units, Hierarchy & Participants

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

        await db.ExecuteInTransactionAsync(IsolationLevel.ReadCommitted, async token =>
        {
            var unit = await db.OrganizationUnits.FirstOrDefaultAsync(
                u => u.OrganizationId == organizationId && u.OrganizationUnitId == unitId, token)
                ?? throw new NotFoundException($"Organization unit #{unitId} was not found.");

            await db.OrganizationMembers
                .Where(m => m.OrganizationId == organizationId && m.OrganizationUnitId == unitId)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.OrganizationUnitId, (int?)null), token);

            db.OrganizationUnits.Remove(unit);
            await db.SaveChangesAsync(token);
        }, ct);
    }

    public async Task<OrganizationHierarchyDto> GetHierarchyAsync(
        int organizationId, bool includeEmails = true, int viewerStudentId = 0, string? viewerRole = null, CancellationToken ct = default)
    {
        var org = await db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrganizationId == organizationId, ct)
            ?? throw new NotFoundException($"Organization with ID {organizationId} was not found.");

        var members = await LoadMembersCoreAsync(organizationId, includeEmails, viewerStudentId, viewerRole, ct);
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

        // Aggregate in SQL, not in memory: this used to load every confirmed registration row for
        // every event the org has ever run, which grows without bound as the org's history grows.
        var registrations = db.EventRegistrations.AsNoTracking()
            .Where(r => r.Event.OrganizationId == organizationId && r.Status == RegistrationStatus.Confirmed);

        var grouped = registrations
            .GroupBy(r => r.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                EventsCount = g.Count(),
                LatestRegisteredAt = g.Max(x => x.RegisteredAt)
            });

        // Join back on the per-student max timestamp to fetch the latest event's title and the
        // student's display fields without materializing the whole registration history.
        var registrationsPage = await (
            from g in grouped
            join r in registrations
                on new { g.StudentId, RegisteredAt = g.LatestRegisteredAt }
                equals new { r.StudentId, r.RegisteredAt }
            select new
            {
                g.StudentId,
                g.EventsCount,
                r.RegisteredAt,
                r.Student.FirstName,
                r.Student.LastName,
                r.Student.AffiliationType,
                r.Student.City,
                FacultyName = r.Student.Faculty != null ? r.Student.Faculty.Name : null,
                UniversityName = r.Student.Faculty != null ? r.Student.Faculty.University.Name : null,
                EventTitle = r.Event.Title
            })
            .ToListAsync(ct);

        // Defensive: identical RegisteredAt timestamps would join twice for one student.
        var latestRows = registrationsPage
            .GroupBy(x => x.StudentId)
            .Select(g => g.First())
            .ToList();

        if (latestRows.Count == 0)
            return [];

        var attendancesByStudent = await db.SessionAttendances.AsNoTracking()
            .Where(a => a.Session.Event.OrganizationId == organizationId)
            .GroupBy(a => a.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count(), LatestCheckIn = g.Max(x => x.CheckInTime) })
            .ToDictionaryAsync(x => x.StudentId, ct);

        return latestRows
            .Select(latest =>
            {
                attendancesByStudent.TryGetValue(latest.StudentId, out var att);
                var affiliationLabel = latest.UniversityName is not null && latest.FacultyName is not null
                    ? $"{latest.FacultyName} · {latest.UniversityName}"
                    : AffiliationFormatting.FormatAffiliationLabel(latest.AffiliationType)
                      + (string.IsNullOrWhiteSpace(latest.City) ? "" : $" · {latest.City}");

                var lastActive = att is not null && att.LatestCheckIn > latest.RegisteredAt
                    ? att.LatestCheckIn
                    : latest.RegisteredAt;

                return new OrganizationParticipantDto(
                    latest.StudentId,
                    latest.FirstName,
                    latest.LastName,
                    affiliationLabel,
                    latest.EventsCount,
                    att?.Count ?? 0,
                    latest.EventTitle,
                    lastActive);
            })
            .OrderByDescending(p => p.SessionsAttendedCount)
            .ThenByDescending(p => p.EventsRegisteredCount)
            .ThenBy(p => p.FirstName)
            .ToList();
    }

    #endregion

    #region Membership Applications & Recruitment

    public async Task<IReadOnlyList<MembershipApplicationDto>> GetMyApplicationsAsync(
        int studentId, CancellationToken ct = default)
    {
        // Bound the history: this list is the caller's own applications across all orgs, and the
        // consumers use it for per-org "did I apply" lookups plus a dashboard summary. 100 recent
        // applications covers every realistic account while keeping the query bounded.
        return await db.MembershipApplications.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.AppliedAt)
            .Take(100)
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
            AppliedAt = _time.GetUtcNow().UtcDateTime
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

        var updatedApplicationId = await db.ExecuteInTransactionAsync(IsolationLevel.ReadCommitted, async token =>
        {
            var application = await db.MembershipApplications
                .Include(x => x.Organization)
                .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.ApplicationId == applicationId, token)
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
                    var previousMembership = await db.OrganizationMembers.IgnoreQueryFilters([QueryFilterNames.SoftDelete])
                        .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.StudentId == application.StudentId, token);
                    if (previousMembership is null)
                    {
                        db.OrganizationMembers.Add(new OrganizationMember
                        {
                            OrganizationId = organizationId,
                            StudentId = application.StudentId,
                            Role = OrganizationRoles.Member,
                            OrganizationUnitId = application.OrganizationUnitId,
                            HierarchyTier = OrganizationHierarchyTier.OrganizingCrew,
                            JoinedAt = _time.GetUtcNow().UtcDateTime
                        });
                    }
                    else
                    {
                        previousMembership.Role = OrganizationRoles.Member;
                        previousMembership.OrganizationUnitId = application.OrganizationUnitId;
                        previousMembership.HierarchyTier = OrganizationHierarchyTier.OrganizingCrew;
                        previousMembership.JoinedAt = _time.GetUtcNow().UtcDateTime;
                        previousMembership.DeletedAt = null;
                    }
                }

                application.Status = decisionStatus;
                application.ReviewedAt = _time.GetUtcNow().UtcDateTime;
                application.AddDomainEvent(new ApplicationReviewedDomainEvent(
                    application.StudentId,
                    application.Organization.Name,
                    "Organization",
                    decisionStatus));
            }

            await db.SaveChangesAsync(token);
            return application.ApplicationId;
        }, ct);

        return await GetApplicationDtoAsync(updatedApplicationId, ct);
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
        application.DeletedAt = _time.GetUtcNow().UtcDateTime;
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

    #endregion

    #region Organization Creation Proposals

    public async Task<OrganizationCreationRequestDto> SubmitOrganizationCreationRequestAsync(
        int requesterStudentId, CreateOrganizationProposalRequest request, CancellationToken ct = default)
    {
        var requester = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == requesterStudentId, ct)
            ?? throw new NotFoundException($"Student with ID {requesterStudentId} was not found.");

        var name = request.Name.Trim();
        if (await db.Organizations.AnyAsync(o => o.Name == name, ct))
            throw new ConflictException($"An organization named '{name}' already exists.");

        var pendingCount = await db.OrganizationCreationRequests
            .CountAsync(r => r.RequesterStudentId == requesterStudentId && r.Status == ApplicationStatus.Pending, ct);
        if (pendingCount >= 5)
            throw new ConflictException("You already have 5 pending organization creation requests. Wait for review or withdraw one.");

        if (await db.OrganizationCreationRequests.AnyAsync(
                r => r.RequesterStudentId == requesterStudentId && r.Name == name && r.Status == ApplicationStatus.Pending, ct))
        {
            throw new ConflictException($"You already have a pending request to create '{name}'.");
        }

        if (request.UniversityId is > 0
            && !await db.Universities.AsNoTracking().AnyAsync(u => u.UniversityId == request.UniversityId.Value, ct))
        {
            throw new NotFoundException($"University with ID {request.UniversityId.Value} was not found.");
        }

        var proposal = new OrganizationCreationRequest
        {
            RequesterStudentId = requester.StudentId,
            Name = name,
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            LogoUrl = NormalizeOptional(request.LogoUrl),
            Scope = request.Scope,
            UniversityId = request.UniversityId is > 0 ? request.UniversityId : null,
            City = NormalizeOptional(request.City),
            CoverImageUrl = NormalizeOptional(request.CoverImageUrl),
            WebsiteUrl = NormalizeOptional(request.WebsiteUrl),
            CurrentSeasonName = NormalizeOptional(request.CurrentSeasonName),
            Justification = NormalizeOptional(request.Justification),
            Status = ApplicationStatus.Pending,
            RequestedAt = _time.GetUtcNow().UtcDateTime,
        };

        db.OrganizationCreationRequests.Add(proposal);
        await db.SaveChangesAsync(ct);

        await auditLog.RecordAsync(
            "OrganizationRequestSubmitted",
            "OrganizationCreationRequest",
            proposal.RequestId.ToString(),
            newValues: new { proposal.RequestId, proposal.Name, proposal.Category, Scope = proposal.Scope.ToString() },
            actorStudentIdOverride: requesterStudentId,
            ct: ct);

        return await GetOrganizationCreationRequestDtoAsync(proposal.RequestId, ct);
    }

    public async Task<IReadOnlyList<OrganizationCreationRequestDto>> GetMyOrganizationCreationRequestsAsync(
        int requesterStudentId, CancellationToken ct = default)
    {
        var items = await db.OrganizationCreationRequests.AsNoTracking()
            .Include(x => x.RequesterStudent).ThenInclude(s => s.Faculty!).ThenInclude(f => f.University)
            .Include(x => x.University)
            .Include(x => x.ReviewedByStudent)
            .Where(x => x.RequesterStudentId == requesterStudentId)
            .OrderByDescending(x => x.RequestedAt)
            .Take(100)
            .ToListAsync(ct);

        return items.Select(MapCreationRequestDto).ToList();
    }

    public async Task WithdrawOrganizationCreationRequestAsync(
        int requestId, int requesterStudentId, CancellationToken ct = default)
    {
        var proposal = await db.OrganizationCreationRequests
            .FirstOrDefaultAsync(x => x.RequestId == requestId, ct)
            ?? throw new NotFoundException($"Organization creation request #{requestId} was not found.");

        if (proposal.RequesterStudentId != requesterStudentId)
            throw new ForbiddenException("You can only withdraw your own organization creation requests.");

        if (proposal.Status != ApplicationStatus.Pending)
            throw new ConflictException("Only pending organization creation requests can be withdrawn.");

        proposal.Status = ApplicationStatus.Withdrawn;
        await db.SaveChangesAsync(ct);

        await auditLog.RecordAsync(
            "OrganizationRequestWithdrawn",
            "OrganizationCreationRequest",
            requestId.ToString(),
            actorStudentIdOverride: requesterStudentId,
            ct: ct);
    }

    public async Task<PagedResult<OrganizationCreationRequestDto>> GetAdminOrganizationCreationRequestsAsync(
        ApplicationStatus? status, string? search, PageRequest page, CancellationToken ct = default)
    {
        var query = db.OrganizationCreationRequests.AsNoTracking()
            .Include(x => x.RequesterStudent).ThenInclude(s => s.Faculty!).ThenInclude(f => f.University)
            .Include(x => x.University)
            .Include(x => x.ReviewedByStudent)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term)
                || x.Category.Contains(term)
                || x.RequesterStudent.FirstName.Contains(term)
                || x.RequesterStudent.LastName.Contains(term)
                || x.RequesterStudent.Email.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Status == ApplicationStatus.Pending ? 0 : 1)
            .ThenByDescending(x => x.RequestedAt)
            .Skip(page.Skip)
            .Take(page.SafePageSize)
            .ToListAsync(ct);

        return new PagedResult<OrganizationCreationRequestDto>(
            items.Select(MapCreationRequestDto).ToList(),
            page.SafePage,
            page.SafePageSize,
            total);
    }

    public async Task<OrganizationCreationRequestDto> ReviewOrganizationCreationRequestAsync(
        int requestId, int reviewerAdminId, ReviewOrganizationProposalRequest review, CancellationToken ct = default)
    {
        var outcome = await db.ExecuteInTransactionAsync(IsolationLevel.ReadCommitted, async token =>
        {
            var proposal = await db.OrganizationCreationRequests
                .Include(x => x.RequesterStudent)
                .FirstOrDefaultAsync(x => x.RequestId == requestId, token)
                ?? throw new NotFoundException($"Organization creation request #{requestId} was not found.");

            if (proposal.Status != ApplicationStatus.Pending)
                throw new ConflictException($"Request #{requestId} has already been reviewed ({proposal.Status}).");

            var promoted = false;
            int? createdOrgId = null;
            string? orgName = null;
            string? rejectedNotes = null;

            if (review.Approve)
            {
                if (await db.Organizations.AnyAsync(o => o.Name == proposal.Name, token))
                    throw new ConflictException($"An organization named '{proposal.Name}' already exists.");

                var organization = new Organization
                {
                    Name = proposal.Name,
                    Description = proposal.Description,
                    Category = proposal.Category,
                    LogoUrl = proposal.LogoUrl,
                    Scope = proposal.Scope,
                    UniversityId = proposal.UniversityId,
                    City = proposal.City,
                    CoverImageUrl = proposal.CoverImageUrl,
                    WebsiteUrl = proposal.WebsiteUrl,
                    IsRecruiting = true,
                    CurrentSeasonName = proposal.CurrentSeasonName ?? "Founding Season",
                };
                db.Organizations.Add(organization);
                await db.SaveChangesAsync(token);

                db.OrganizationMembers.Add(new OrganizationMember
                {
                    OrganizationId = organization.OrganizationId,
                    StudentId = proposal.RequesterStudentId,
                    Role = OrganizationRoles.President,
                    HierarchyTier = OrganizationHierarchyTier.HighBoard,
                    JoinedAt = _time.GetUtcNow().UtcDateTime,
                });

                if (proposal.RequesterStudent.Role == UserRole.Student)
                {
                    proposal.RequesterStudent.Role = UserRole.OrgAdmin;
                    proposal.RequesterStudent.SecurityStamp = Guid.NewGuid();
                    proposal.RequesterStudent.RefreshToken = null;
                    proposal.RequesterStudent.RefreshTokenExpiresAt = null;
                    promoted = true;
                }

                proposal.Status = ApplicationStatus.Accepted;
                proposal.ReviewedAt = _time.GetUtcNow().UtcDateTime;
                proposal.ReviewedByStudentId = reviewerAdminId;
                proposal.ReviewNotes = NormalizeOptional(review.ReviewNotes);
                proposal.CreatedOrganizationId = organization.OrganizationId;

                db.Notifications.Add(new Notification
                {
                    StudentId = proposal.RequesterStudentId,
                    Title = "Organization Request Approved",
                    Message = $"Your request to launch '{proposal.Name}' was approved! You are now its President and have been promoted to Organization Admin.",
                    Type = NotificationType.ApplicationStatus,
                    IsRead = false,
                    CreatedAt = _time.GetUtcNow().UtcDateTime,
                });

                createdOrgId = organization.OrganizationId;
                orgName = organization.Name;
            }
            else
            {
                rejectedNotes = NormalizeOptional(review.ReviewNotes);
                proposal.Status = ApplicationStatus.Rejected;
                proposal.ReviewedAt = _time.GetUtcNow().UtcDateTime;
                proposal.ReviewedByStudentId = reviewerAdminId;
                proposal.ReviewNotes = rejectedNotes;

                db.Notifications.Add(new Notification
                {
                    StudentId = proposal.RequesterStudentId,
                    Title = "Organization Request Reviewed",
                    Message = rejectedNotes is null
                        ? $"Your request to create '{proposal.Name}' was declined by an administrator."
                        : $"Your request to create '{proposal.Name}' was declined: {rejectedNotes}",
                    Type = NotificationType.ApplicationStatus,
                    IsRead = false,
                    CreatedAt = _time.GetUtcNow().UtcDateTime,
                });
            }

            await db.SaveChangesAsync(token);
            return (proposal.RequestId, proposal.RequesterStudentId, promoted, createdOrgId, orgName, rejectedNotes);
        }, ct);

        if (review.Approve)
        {
            if (outcome.promoted)
                stampValidator.Invalidate(outcome.RequesterStudentId);

            await auditLog.RecordAsync(
                "OrganizationRequestApproved",
                "OrganizationCreationRequest",
                outcome.RequestId.ToString(),
                oldValues: new { Status = nameof(ApplicationStatus.Pending) },
                newValues: new
                {
                    Status = nameof(ApplicationStatus.Accepted),
                    CreatedOrganizationId = outcome.createdOrgId,
                    OrganizationName = outcome.orgName,
                    outcome.RequesterStudentId,
                    PromotedToOrgAdmin = outcome.promoted,
                },
                actorStudentIdOverride: reviewerAdminId > 0 ? reviewerAdminId : null,
                ct: ct);
        }
        else
        {
            await auditLog.RecordAsync(
                "OrganizationRequestRejected",
                "OrganizationCreationRequest",
                outcome.RequestId.ToString(),
                oldValues: new { Status = nameof(ApplicationStatus.Pending) },
                newValues: new { Status = nameof(ApplicationStatus.Rejected), ReviewNotes = outcome.rejectedNotes },
                actorStudentIdOverride: reviewerAdminId > 0 ? reviewerAdminId : null,
                ct: ct);
        }

        return await GetOrganizationCreationRequestDtoAsync(outcome.RequestId, ct);
    }

    private async Task<OrganizationCreationRequestDto> GetOrganizationCreationRequestDtoAsync(int requestId, CancellationToken ct)
    {
        var entity = await db.OrganizationCreationRequests.AsNoTracking()
            .Include(x => x.RequesterStudent).ThenInclude(s => s.Faculty!).ThenInclude(f => f.University)
            .Include(x => x.University)
            .Include(x => x.ReviewedByStudent)
            .FirstOrDefaultAsync(x => x.RequestId == requestId, ct)
            ?? throw new NotFoundException($"Organization creation request #{requestId} was not found.");

        return MapCreationRequestDto(entity);
    }

    private static OrganizationCreationRequestDto MapCreationRequestDto(OrganizationCreationRequest x) =>
        new(
            x.RequestId,
            x.RequesterStudentId,
            $"{x.RequesterStudent.FirstName} {x.RequesterStudent.LastName}".Trim(),
            x.RequesterStudent.Email,
            x.RequesterStudent.Faculty?.University?.Name,
            x.Name,
            x.Description,
            x.Category,
            x.LogoUrl,
            x.Scope,
            x.UniversityId,
            x.University?.Name,
            x.City,
            x.CoverImageUrl,
            x.WebsiteUrl,
            x.CurrentSeasonName,
            x.Justification,
            x.Status,
            x.RequestedAt,
            x.ReviewedAt,
            x.ReviewedByStudentId,
            x.ReviewedByStudent is null ? null : $"{x.ReviewedByStudent.FirstName} {x.ReviewedByStudent.LastName}".Trim(),
            x.ReviewNotes,
            x.CreatedOrganizationId);

    #endregion

    #region Helpers

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #endregion
}
