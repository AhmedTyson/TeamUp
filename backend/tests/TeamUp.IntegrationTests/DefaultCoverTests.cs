// Date: 2026-11-25
// Purpose: Round-5 tests for generated default covers: the 10 seeded system covers are served as PNGs,
// and organization/event public summaries expose deterministic cover URLs that resolve publicly.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TeamUp.Application.Events;
using TeamUp.Application.Organizations;

namespace TeamUp.IntegrationTests;

public sealed class DefaultCoverTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    [Fact]
    public async Task DefaultCovers_AreSeededAndServedAsPng()
    {
        using var client = factory.CreateClient();
        for (var i = 1; i <= 10; i++)
        {
            var url = $"/api/v1/images/c0e70000-0000-4000-8000-0000000000{i:00}";
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Assert.True(bytes.Length > 1_000, $"cover {i} too small ({bytes.Length} bytes)");
        }
    }

    private HttpClient OrgAdminClient(int id)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(id, $"cover{id}@test.com", "OrgAdmin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task PublicOrganizations_ExposeDeterministicCoverUrls_ThatResolvePublicly()
    {
        using var admin = OrgAdminClient(731);
        var create = await admin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"CoverOrg_{Guid.NewGuid():N}", "cover host", "Technology", null));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        using var client = factory.CreateClient();
        var first = await client.GetAsync("/api/v1/organizations/public?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        using var doc = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").GetProperty("items");
        Assert.True(items.GetArrayLength() > 0);

        var covers = new Dictionary<int, string>();
        foreach (var item in items.EnumerateArray())
        {
            var id = item.GetProperty("organizationId").GetInt32();
            var cover = item.GetProperty("coverImageUrl").GetString();
            Assert.NotNull(cover);
            Assert.Matches(@"^/api/v1/images/[0-9a-fA-F-]{36}$", cover);
            covers[id] = cover;

            var image = await client.GetAsync(cover);
            Assert.Equal(HttpStatusCode.OK, image.StatusCode);
            Assert.Equal("image/png", image.Content.Headers.ContentType?.MediaType);
        }

        // Deterministic: the same query later assigns the same cover to each organization.
        var second = await client.GetAsync("/api/v1/organizations/public?page=1&pageSize=10");
        using var doc2 = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        foreach (var item in doc2.RootElement.GetProperty("data").GetProperty("items").EnumerateArray())
        {
            var id = item.GetProperty("organizationId").GetInt32();
            if (covers.TryGetValue(id, out var cover))
            {
                Assert.Equal(cover, item.GetProperty("coverImageUrl").GetString());
            }
        }
    }

    [Fact]
    public async Task PublicEvents_ExposeCoverUrls_ThatResolvePublicly()
    {
        using var admin = OrgAdminClient(732);
        var createOrg = await admin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"CoverEventOrg_{Guid.NewGuid():N}", "cover host", "Technology", null));
        Assert.Equal(HttpStatusCode.Created, createOrg.StatusCode);
        using var orgDoc = JsonDocument.Parse(await createOrg.Content.ReadAsStringAsync());
        var orgId = orgDoc.RootElement.GetProperty("data").GetProperty("organizationId").GetInt32();

        var start = DateTime.UtcNow.AddDays(1);
        var createEvent = await admin.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest($"CoverEvent_{Guid.NewGuid():N}", "desc", TeamUp.Domain.Enums.EventType.Workshop,
                "Cairo", start, start.AddHours(2), 30, orgId));
        Assert.Equal(HttpStatusCode.Created, createEvent.StatusCode);

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/events/public?page=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").GetProperty("items");
        foreach (var item in items.EnumerateArray())
        {
            var cover = item.GetProperty("coverImageUrl").GetString();
            Assert.NotNull(cover);
            Assert.Matches(@"^/api/v1/images/[0-9a-fA-F-]{36}$", cover);
            var image = await client.GetAsync(cover);
            Assert.Equal(HttpStatusCode.OK, image.StatusCode);
        }
    }

    [Fact]
    public async Task OrganizationPublicDetail_ReturnsDeterministicDefaultCoverMatchingList()
    {
        using var admin = OrgAdminClient(733);
        var createOrg = await admin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"DetailCoverOrg_{Guid.NewGuid():N}", "detail cover host", "Technology", null));
        Assert.Equal(HttpStatusCode.Created, createOrg.StatusCode);
        using var createdDoc = JsonDocument.Parse(await createOrg.Content.ReadAsStringAsync());
        var orgId = createdDoc.RootElement.GetProperty("data").GetProperty("organizationId").GetInt32();

        using var client = factory.CreateClient();
        var listRes = await client.GetAsync("/api/v1/organizations/public?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);
        using var listDoc = JsonDocument.Parse(await listRes.Content.ReadAsStringAsync());
        var listCover = listDoc.RootElement.GetProperty("data").GetProperty("items")
            .EnumerateArray()
            .First(x => x.GetProperty("organizationId").GetInt32() == orgId)
            .GetProperty("coverImageUrl")
            .GetString();

        var detailRes = await client.GetAsync($"/api/v1/organizations/public/{orgId}");
        Assert.Equal(HttpStatusCode.OK, detailRes.StatusCode);
        using var detailDoc = JsonDocument.Parse(await detailRes.Content.ReadAsStringAsync());
        var detailCover = detailDoc.RootElement.GetProperty("data").GetProperty("coverImageUrl").GetString();

        Assert.NotNull(detailCover);
        Assert.StartsWith("/api/v1/images/", detailCover, StringComparison.Ordinal);
        Assert.Equal(listCover, detailCover);
    }
}
