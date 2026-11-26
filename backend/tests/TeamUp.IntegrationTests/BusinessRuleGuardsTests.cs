// Date: 2026-11-25
// Purpose: Business-rule guards from the continued bug hunt: registration closes when an event has
// ended, and an organization can never lose its last administrator (removal or demotion), which
// would orphan it with nobody able to manage events, members, or recruitment.

using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Events;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public class BusinessRuleGuardsTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly ApiHelper _api = new(factory);

    private (HttpClient Client, int StudentId) OrgAdmin(string prefix)
    {
        var id = Random.Shared.Next(100000, 999999);
        return (_api.Authed(factory.CreateToken(id, $"{prefix}_{Guid.NewGuid():N}@teamup.test", "OrgAdmin")), id);
    }

    private async Task<int> CreateOrgAsync(HttpClient orgAdmin, string name)
    {
        var org = await ApiHelper.DataAsync<OrganizationDetailDto>(await orgAdmin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"{name}_{Guid.NewGuid():N}", "org", "Technology", null), ApiHelper.Json));
        return org.OrganizationId;
    }

    [Fact]
    public async Task Register_ForEndedEvent_IsRejected()
    {
        var (orgAdmin, _) = OrgAdmin("endedevt");
        var orgId = await CreateOrgAsync(orgAdmin, "EndedEvt");
        var ev = await ApiHelper.DataAsync<EventDto>(await orgAdmin.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Past Workshop", "d", EventType.Workshop, "Hall",
                DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), 10, orgId), ApiHelper.Json));

        var (studentClient, _, _, _) = await _api.RegisterAsync("endedreg");
        var res = await studentClient.PostAsync($"/api/v1/events/{ev.EventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Fact]
    public async Task Register_ForLiveEvent_StillWorks()
    {
        var (orgAdmin, _) = OrgAdmin("liveevt");
        var orgId = await CreateOrgAsync(orgAdmin, "LiveEvt");
        var ev = await ApiHelper.DataAsync<EventDto>(await orgAdmin.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Future Workshop", "d", EventType.Workshop, "Hall",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, orgId), ApiHelper.Json));

        var (studentClient, _, _, _) = await _api.RegisterAsync("livereg");
        var res = await studentClient.PostAsync($"/api/v1/events/{ev.EventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_LastAdmin_IsRejected_UntilSuccessorExists()
    {
        var (orgAdmin, orgAdminId) = OrgAdmin("lastadmin");
        var orgId = await CreateOrgAsync(orgAdmin, "LastAdmin");

        // Self-removal as the only administrator must be rejected.
        var loneRemove = await orgAdmin.DeleteAsync($"/api/v1/organizations/{orgId}/members/{orgAdminId}");
        Assert.Equal(HttpStatusCode.Conflict, loneRemove.StatusCode);

        // Add a second member and promote them to an admin role.
        var (_, memberId, _, _) = await _api.RegisterAsync("successor");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationId = orgId,
                StudentId = memberId,
                Role = OrganizationRoles.Member,
                JoinedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var promote = await orgAdmin.PatchAsJsonAsync($"/api/v1/organizations/{orgId}/members/{memberId}",
            new UpdateOrganizationMemberRequest(OrganizationRoles.Admin));
        Assert.Equal(HttpStatusCode.OK, promote.StatusCode);

        // Now self-removal succeeds: the org still has an administrator.
        var remove = await orgAdmin.DeleteAsync($"/api/v1/organizations/{orgId}/members/{orgAdminId}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_DemotingLastAdmin_IsRejected()
    {
        var (orgAdmin, orgAdminId) = OrgAdmin("demotelast");
        var orgId = await CreateOrgAsync(orgAdmin, "DemoteLast");

        var demote = await orgAdmin.PatchAsJsonAsync($"/api/v1/organizations/{orgId}/members/{orgAdminId}",
            new UpdateOrganizationMemberRequest(OrganizationRoles.Member));
        Assert.Equal(HttpStatusCode.Conflict, demote.StatusCode);

        // Re-confirming another admin role on self is fine (still an admin).
        var keep = await orgAdmin.PatchAsJsonAsync($"/api/v1/organizations/{orgId}/members/{orgAdminId}",
            new UpdateOrganizationMemberRequest(OrganizationRoles.President));
        Assert.Equal(HttpStatusCode.OK, keep.StatusCode);
    }
}
