using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
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

        var body = await res.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(body);
        Assert.Equal("Healthy", body["status"]?.ToString());
        Assert.Equal("Connected", body["database"]?.ToString());
    }

    [Fact]
    public async Task HealthLiveAndReady_ReturnOkWithExpectedPayload()
    {
        var liveRes = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, liveRes.StatusCode);
        var liveJson = await liveRes.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(liveJson);
        Assert.Equal("Healthy", liveJson["status"]?.ToString());

        var readyRes = await _client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, readyRes.StatusCode);
        var readyJson = await readyRes.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(readyJson);
        Assert.Equal("Healthy", readyJson["status"]?.ToString());
        Assert.Equal("Connected", readyJson["database"]?.ToString());
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
