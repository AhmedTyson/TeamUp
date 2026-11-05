using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class OrganizationService(IAppDbContext db) : IOrganizationService
{
    public async Task<PagedResult<OrganizationSummaryDto>> GetOrganizationsAsync(
        string? search, string? category, PageRequest page, CancellationToken ct = default)
    {
        var query = db.Organizations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Description.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            var value = category.Trim();
            query = query.Where(x => x.Category == value);
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Name)
            .Skip(page.Skip).Take(page.SafePageSize)
            .Select(x => new OrganizationSummaryDto(
                x.OrganizationId, x.Name, x.Description, x.Category, x.LogoUrl,
                db.OrganizationMembers.Count(m => m.OrganizationId == x.OrganizationId)))
            .ToListAsync(ct);
        return new PagedResult<OrganizationSummaryDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public async Task<OrganizationDetailDto> GetOrganizationByIdAsync(int id, CancellationToken ct = default)
    {
        var organization = await db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrganizationId == id, ct)
            ?? throw new NotFoundException($"Organization with ID {id} was not found.");
        var members = await GetMembersAsync(id, ct);
        var events = await db.Events.AsNoTracking()
            .Where(x => x.OrganizationId == id)
            .OrderBy(x => x.StartDate)
            .Select(x => new OrganizationEventSummaryDto(
                x.EventId, x.Title, x.Type, x.Location, x.StartDate, x.EndDate))
            .ToListAsync(ct);
        return new OrganizationDetailDto(organization.OrganizationId, organization.Name,
            organization.Description, organization.Category, organization.LogoUrl, members, events);
    }

    public async Task<OrganizationDetailDto> CreateOrganizationAsync(CreateOrganizationRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (await db.Organizations.AnyAsync(x => x.Name == name, ct))
            throw new ConflictException($"An organization named '{name}' already exists.");

        var organization = new Organization
        {
            Name = name,
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            LogoUrl = NormalizeOptional(request.LogoUrl)
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync(ct);
        return new OrganizationDetailDto(organization.OrganizationId, organization.Name,
            organization.Description, organization.Category, organization.LogoUrl, [], []);
    }

    public async Task<OrganizationDetailDto> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request, CancellationToken ct = default)
    {
        var organization = await db.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == id, ct)
            ?? throw new NotFoundException($"Organization with ID {id} was not found.");
        var name = request.Name.Trim();
        if (!string.Equals(organization.Name, name, StringComparison.OrdinalIgnoreCase)
            && await db.Organizations.AnyAsync(x => x.OrganizationId != id && x.Name == name, ct))
            throw new ConflictException($"An organization named '{name}' already exists.");

        organization.Name = name;
        organization.Description = request.Description.Trim();
        organization.Category = request.Category.Trim();
        organization.LogoUrl = NormalizeOptional(request.LogoUrl);
        await db.SaveChangesAsync(ct);
        return await GetOrganizationByIdAsync(id, ct);
    }

    public async Task DeleteOrganizationAsync(int id, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var organization = await db.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == id, ct)
            ?? throw new NotFoundException($"Organization with ID {id} was not found.");
        var members = await db.OrganizationMembers.Where(x => x.OrganizationId == id).ToListAsync(ct);
        var applications = await db.MembershipApplications.Where(x => x.OrganizationId == id).ToListAsync(ct);
        var events = await db.Events.Include(x => x.Sessions)
            .Where(x => x.OrganizationId == id).ToListAsync(ct);

        db.OrganizationMembers.RemoveRange(members);
        db.MembershipApplications.RemoveRange(applications);
        foreach (var eventEntity in events)
            db.EventSessions.RemoveRange(eventEntity.Sessions);
        db.Events.RemoveRange(events);
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<OrganizationMemberDto>> GetMembersAsync(int organizationId, CancellationToken ct = default)
    {
        if (!await db.Organizations.AnyAsync(x => x.OrganizationId == organizationId, ct))
            throw new NotFoundException($"Organization with ID {organizationId} was not found.");
        return await db.OrganizationMembers.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Role).ThenBy(x => x.JoinedAt)
            .Select(x => new OrganizationMemberDto(x.StudentId, x.Student.FirstName,
                x.Student.LastName, x.Student.Email, x.Role, x.JoinedAt))
            .ToListAsync(ct);
    }

    public async Task<OrganizationMemberDto> UpdateMemberRoleAsync(
        int organizationId, int studentId, UpdateOrganizationMemberRequest request, CancellationToken ct = default)
    {
        var member = await db.OrganizationMembers
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.StudentId == studentId, ct)
            ?? throw new NotFoundException("The requested organization member was not found.");
        member.Role = request.Role.Trim();
        await db.SaveChangesAsync(ct);
        return ToMemberDto(member);
    }

    public async Task RemoveMemberAsync(int organizationId, int studentId, CancellationToken ct = default)
    {
        var member = await db.OrganizationMembers.FirstOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.StudentId == studentId, ct)
            ?? throw new NotFoundException("The requested organization member was not found.");

        db.OrganizationMembers.Remove(member);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MembershipApplicationDto>> GetApplicationsAsync(int organizationId, CancellationToken ct = default)
    {
        if (!await db.Organizations.AnyAsync(x => x.OrganizationId == organizationId, ct))
            throw new NotFoundException($"Organization with ID {organizationId} was not found.");
        return await db.MembershipApplications.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.AppliedAt)
            .Select(x => new MembershipApplicationDto(x.ApplicationId, x.OrganizationId, x.StudentId,
                x.Student.FirstName, x.Student.LastName, x.Student.Email, x.Message, x.Status, x.AppliedAt, x.ReviewedAt))
            .ToListAsync(ct);
    }

    public async Task<MembershipApplicationDto> ApplyForMembershipAsync(
        int organizationId, int studentId, CreateMembershipApplicationRequest request, CancellationToken ct = default)
    {
        if (!await db.Organizations.AnyAsync(x => x.OrganizationId == organizationId, ct))
            throw new NotFoundException($"Organization with ID {organizationId} was not found.");
        if (!await db.Students.AnyAsync(x => x.StudentId == studentId, ct))
            throw new NotFoundException($"Student with ID {studentId} was not found.");
        if (await db.OrganizationMembers.AnyAsync(x => x.OrganizationId == organizationId && x.StudentId == studentId, ct))
            throw new ConflictException("You are already a member of this organization.");
        if (await db.MembershipApplications.AnyAsync(x => x.OrganizationId == organizationId
            && x.StudentId == studentId && x.Status == ApplicationStatus.Pending, ct))
            throw new ConflictException("You already have a pending application for this organization.");

        var application = new MembershipApplication
        {
            OrganizationId = organizationId,
            StudentId = studentId,
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };
        db.MembershipApplications.Add(application);
        await db.SaveChangesAsync(ct);
        return await GetApplicationDtoAsync(application.ApplicationId, ct);
    }

    public async Task<MembershipApplicationDto> ReviewApplicationAsync(
        int organizationId, int applicationId, ReviewMembershipApplicationRequest request, CancellationToken ct = default)
    {
        if (request.Status is not (ApplicationStatus.Accepted or ApplicationStatus.Rejected))
            throw new BadRequestException("An application can only be accepted or rejected by an organization admin.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var application = await db.MembershipApplications
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.ApplicationId == applicationId, ct)
            ?? throw new NotFoundException("Membership application was not found.");
        if (application.Status != ApplicationStatus.Pending)
            throw new ConflictException("Only pending applications can be reviewed.");

        if (request.Status == ApplicationStatus.Accepted)
        {
            var previousMembership = await db.OrganizationMembers.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.StudentId == application.StudentId, ct);
            if (previousMembership is null)
            {
                db.OrganizationMembers.Add(new OrganizationMember
                {
                    OrganizationId = organizationId,
                    StudentId = application.StudentId,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow
                });
            }
            else
            {
                previousMembership.Role = "Member";
                previousMembership.JoinedAt = DateTime.UtcNow;
                previousMembership.DeletedAt = null;
            }
        }

        application.Status = request.Status;
        application.ReviewedAt = DateTime.UtcNow;
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
            throw new UnauthorizedException("You are not allowed to withdraw this application.");
        if (application.Status != ApplicationStatus.Pending)
            throw new ConflictException("Only pending applications can be withdrawn.");

        application.Status = ApplicationStatus.Withdrawn;
        application.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task<MembershipApplicationDto> GetApplicationDtoAsync(int applicationId, CancellationToken ct)
    {
        var app = await db.MembershipApplications.AsNoTracking()
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId, ct)
            ?? throw new NotFoundException("Membership application was not found.");
        return new MembershipApplicationDto(
            app.ApplicationId, app.OrganizationId, app.StudentId,
            app.Student.FirstName, app.Student.LastName, app.Student.Email,
            app.Message, app.Status, app.AppliedAt, app.ReviewedAt);
    }

    private static OrganizationMemberDto ToMemberDto(OrganizationMember member) =>
        new(member.StudentId, member.Student.FirstName, member.Student.LastName, member.Student.Email, member.Role, member.JoinedAt);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
