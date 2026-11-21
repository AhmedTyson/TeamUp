using System.Net;
using System.Net.Http.Json;
using TeamUp.Application.Common.Models;

namespace TeamUp.IntegrationTests;

public class HealthTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var res = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task UnknownRoute_ReturnsReadableJsonError()
    {
        var res = await _client.GetAsync("/api/v1/nope");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("application/json", res.Content.Headers.ContentType?.MediaType);

        var err = await res.Content.ReadFromJsonAsync<BaseResponse>();
        Assert.NotNull(err);
        Assert.False(err.Success);
        Assert.Equal(404, err.StatusCode);
        Assert.Contains("not found", err.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CorsPreflight_AllowedFrontendOrigin_ReturnsAllowOriginHeader()
    {
        using var req = new HttpRequestMessage(HttpMethod.Options, "/api/v1/universities/catalog");
        req.Headers.Add("Origin", "http://localhost:5173");
        req.Headers.Add("Access-Control-Request-Method", "GET");

        var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.True(res.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains("http://localhost:5173", values);
    }

    [Fact]
    public async Task CorsPreflight_DisallowedOrigin_OmitsAllowOriginHeader()
    {
        using var req = new HttpRequestMessage(HttpMethod.Options, "/api/v1/universities/catalog");
        req.Headers.Add("Origin", "https://untrusted.example");
        req.Headers.Add("Access-Control-Request-Method", "GET");

        var res = await _client.SendAsync(req);

        Assert.False(res.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
