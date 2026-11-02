using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

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
    public async Task UnknownRoute_ReturnsProblemJson()
    {
        var res = await _client.GetAsync("/api/v1/nope");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("application/problem+json", res.Content.Headers.ContentType?.MediaType);
    }
}
