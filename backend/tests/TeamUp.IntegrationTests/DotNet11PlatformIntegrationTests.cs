// Date: 2026-11-27
// Purpose: Integration tests verifying ASP.NET Core 11 native Zstandard (zstd) compression and typed flat [FromQuery] filter binding.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TeamUp.IntegrationTests;

public sealed class DotNet11PlatformIntegrationTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    [Fact]
    public async Task ResponseCompression_SupportsZstandardEncoding_ForPublicCatalogEndpoints()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/universities");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("zstd", response.Content.Headers.ContentEncoding);

        var compressedBytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(compressedBytes.Length >= 4, "Compressed zstd response should contain a valid frame header.");
        // RFC 8878 Zstandard magic number: 0x28 0xB5 0x2F 0xFD
        Assert.Equal((byte)0x28, compressedBytes[0]);
        Assert.Equal((byte)0xB5, compressedBytes[1]);
        Assert.Equal((byte)0x2F, compressedBytes[2]);
        Assert.Equal((byte)0xFD, compressedBytes[3]);
    }

    [Fact]
    public async Task TypedFromQueryFilterModels_BindFlatQueryParameters_AcrossEventsOrganizationsAndAdmin()
    {
        using var anonClient = factory.CreateClient();

        using var eventsRes = await anonClient.GetAsync("/api/v1/events/public?search=nonexistent&page=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, eventsRes.StatusCode);
        using (var eventsDoc = JsonDocument.Parse(await eventsRes.Content.ReadAsStringAsync()))
        {
            var data = eventsDoc.RootElement.GetProperty("data");
            Assert.Equal(5, data.GetProperty("pageSize").GetInt32());
        }

        using var orgsRes = await anonClient.GetAsync("/api/v1/organizations/public?search=nonexistent&recruitingOnly=true&page=1&pageSize=7");
        Assert.Equal(HttpStatusCode.OK, orgsRes.StatusCode);
        using (var orgsDoc = JsonDocument.Parse(await orgsRes.Content.ReadAsStringAsync()))
        {
            var data = orgsDoc.RootElement.GetProperty("data");
            Assert.Equal(7, data.GetProperty("pageSize").GetInt32());
        }

        using var adminClient = factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateAdminToken());

        using var logsRes = await adminClient.GetAsync("/api/v1/admin/audit-logs?action=RoleChanged&page=1&pageSize=3");
        Assert.Equal(HttpStatusCode.OK, logsRes.StatusCode);
        using (var logsDoc = JsonDocument.Parse(await logsRes.Content.ReadAsStringAsync()))
        {
            var data = logsDoc.RootElement.GetProperty("data");
            Assert.Equal(3, data.GetProperty("pageSize").GetInt32());
        }
    }
}
