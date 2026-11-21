// Date: 2026-11-21
// Purpose: Resource-based multi-tenant authorization service preventing organization/event/session BOLA and ID-existence oracles (STRIDE THR-T-01, THR-T-02, THR-E-01).

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Common.Security;

public interface IOrganizationAuthorizationService
{
    Task<bool> IsOrgAdminAsync(int organizationId, int studentId, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetAdminOrganizationIdsAsync(int studentId, CancellationToken ct = default);
    Task EnsureCanManageOrganizationAsync(int organizationId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task EnsureCanManageEventAsync(int eventId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task EnsureCanManageSessionAsync(int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default);
}

public sealed class OrganizationAuthorizationService(IAppDbContext db) : IOrganizationAuthorizationService
{
    private static bool IsPlatformAdmin(string? role) =>
        string.Equals(role, nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase);

    public async Task<bool> IsOrgAdminAsync(int organizationId, int studentId, CancellationToken ct = default)
    {
        if (studentId <= 0)
            return false;

        var roles = await db.OrganizationMembers
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId && m.StudentId == studentId)
            .Select(m => m.Role)
            .ToListAsync(ct);

        return roles.Any(OrganizationRoles.IsAdminRole);
    }

    public async Task<IReadOnlyList<int>> GetAdminOrganizationIdsAsync(int studentId, CancellationToken ct = default)
    {
        if (studentId <= 0)
            return [];

        var memberships = await db.OrganizationMembers
            .AsNoTracking()
            .Where(m => m.StudentId == studentId)
            .Select(m => new { m.OrganizationId, m.Role })
            .ToListAsync(ct);

        return memberships
            .Where(m => OrganizationRoles.IsAdminRole(m.Role))
            .Select(m => m.OrganizationId)
            .Distinct()
            .ToList();
    }

    public async Task EnsureCanManageOrganizationAsync(
        int organizationId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        if (IsPlatformAdmin(actorRole))
        {
            if (!await db.Organizations.AsNoTracking().AnyAsync(o => o.OrganizationId == organizationId, ct))
                throw new NotFoundException($"Organization with ID {organizationId} was not found.");
            return;
        }

        if (!await IsOrgAdminAsync(organizationId, actorStudentId, ct))
            throw new ForbiddenException("You do not have administrative authority over this organization.");

        if (!await db.Organizations.AsNoTracking().AnyAsync(o => o.OrganizationId == organizationId, ct))
            throw new NotFoundException($"Organization with ID {organizationId} was not found.");
    }

    public async Task EnsureCanManageEventAsync(
        int eventId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        var orgId = await db.Events
            .AsNoTracking()
            .Where(e => e.EventId == eventId)
            .Select(e => (int?)e.OrganizationId)
            .FirstOrDefaultAsync(ct);

        if (IsPlatformAdmin(actorRole))
        {
            if (orgId is null)
                throw new NotFoundException($"Event with ID {eventId} was not found.");
            return;
        }

        if (orgId is null)
        {
            var adminOrgs = await GetAdminOrganizationIdsAsync(actorStudentId, ct);
            if (adminOrgs.Count == 0)
                throw new ForbiddenException("You do not have administrative authority over this event.");
            throw new NotFoundException($"Event with ID {eventId} was not found.");
        }

        if (!await IsOrgAdminAsync(orgId.Value, actorStudentId, ct))
            throw new ForbiddenException("You do not have administrative authority over the organization hosting this event.");
    }

    public async Task EnsureCanManageSessionAsync(
        int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        var orgId = await db.EventSessions
            .AsNoTracking()
            .Where(s => s.SessionId == sessionId)
            .Select(s => (int?)s.Event.OrganizationId)
            .FirstOrDefaultAsync(ct);

        if (IsPlatformAdmin(actorRole))
        {
            if (orgId is null)
                throw new NotFoundException($"Session with ID {sessionId} was not found.");
            return;
        }

        if (orgId is null)
        {
            var adminOrgs = await GetAdminOrganizationIdsAsync(actorStudentId, ct);
            if (adminOrgs.Count == 0)
                throw new ForbiddenException("You do not have administrative authority over this session.");
            throw new NotFoundException($"Session with ID {sessionId} was not found.");
        }

        if (!await IsOrgAdminAsync(orgId.Value, actorStudentId, ct))
            throw new ForbiddenException("You do not have administrative authority over the organization hosting this session.");
    }
}
