// Date: 2026-11-25
// Purpose: Round-4 tests for database-stored images: upload validates magic bytes, stores content as a row, and
// serves it back publicly at /api/v1/images/{id} with the detected content type.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TeamUp.IntegrationTests;

public sealed class ImageStorageTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    // 1x1 transparent PNG.
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private HttpClient AuthedClient(int studentId)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(studentId, $"img{studentId}@test.com", "Student");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static MultipartFormDataContent FormWith(byte[] bytes, string fileName, string contentType)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);
        return form;
    }

    [Fact]
    public async Task Upload_StoresImageInDatabase_AndGetServesBytesPublicly()
    {
        using var client = AuthedClient(701);
        var upload = await client.PostAsync("/api/v1/images", FormWith(TinyPng, "tiny.png", "image/png"));
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);

        using var doc = System.Text.Json.JsonDocument.Parse(await upload.Content.ReadAsStringAsync());
        var url = doc.RootElement.GetProperty("data").GetProperty("url").GetString();
        Assert.NotNull(url);
        Assert.Matches(@"^/api/v1/images/[0-9a-fA-F-]{36}$", url);

        using var anonymous = factory.CreateClient();
        var get = await anonymous.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal("image/png", get.Content.Headers.ContentType?.MediaType);
        Assert.Equal("cross-origin", string.Join(",", get.Headers.GetValues("Cross-Origin-Resource-Policy")));
        Assert.Contains("img-src 'self'", string.Join(",", get.Headers.GetValues("Content-Security-Policy")));
        Assert.Equal(TinyPng, await get.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Upload_RejectsNonImageBytes()
    {
        using var client = AuthedClient(702);
        var response = await client.PostAsync("/api/v1/images",
            FormWith("not an image"u8.ToArray(), "fake.png", "image/png"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_UnknownImage_Returns404()
    {
        using var anonymous = factory.CreateClient();
        var response = await anonymous.GetAsync($"/api/v1/images/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Upload_StoredImageUrl_CanBeUsedAsOrganizationLogoUrl()
    {
        using var admin = factory.CreateClient();
        var adminToken = factory.CreateToken(998, "admin998@teamup.test", "PlatformAdmin");
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var upload = await admin.PostAsync("/api/v1/images", FormWith(TinyPng, "logo.png", "image/png"));
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        using var doc = System.Text.Json.JsonDocument.Parse(await upload.Content.ReadAsStringAsync());
        var logoUrl = doc.RootElement.GetProperty("data").GetProperty("url").GetString();
        Assert.NotNull(logoUrl);

        var createOrg = await admin.PostAsJsonAsync(
            "/api/v1/organizations",
            new TeamUp.Application.Organizations.CreateOrganizationRequest(
                $"ImgLogoOrg_{Guid.NewGuid():N}"[..16],
                "Organization testing stored image logo URL acceptance.",
                "Engineering",
                logoUrl));
        Assert.Equal(HttpStatusCode.Created, createOrg.StatusCode);
    }
}
