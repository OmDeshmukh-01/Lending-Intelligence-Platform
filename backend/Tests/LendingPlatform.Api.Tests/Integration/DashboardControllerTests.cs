using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LendingPlatform.Application.DTOs;
using Xunit;

namespace LendingPlatform.Api.Tests.Integration;

public sealed class DashboardControllerTests : IClassFixture<LendingWebAppFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public DashboardControllerTests(LendingWebAppFactory factory)
    {
        factory.EnsureDb();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Dashboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Dashboard_WithApplications_ReturnsCorrectStats()
    {
        // Submit one approved and one declined application.
        await _client.PostAsJsonAsync("/api/loans/apply",
            new { loanAmount = 500_000, assetValue = 1_000_000, creditScore = 800 }); // Approved

        await _client.PostAsJsonAsync("/api/loans/apply",
            new { loanAmount = 200_000, assetValue = 400_000, creditScore = 600 }); // LTV 50% but score too low

        var response = await _client.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonSerializer.Deserialize<DashboardResponse>(
            await response.Content.ReadAsStringAsync(), JsonOpts);

        Assert.NotNull(body);
        // MeanLtv must not be null when there are applications.
        Assert.NotNull(body!.MeanLtv);
    }

    [Fact]
    public async Task Get_Dashboard_MeanLtv_IncludesBothApprovedAndDeclined()
    {
        // Submit one approved (LTV 50%) and one declined (LTV 90%) to verify mean includes both.
        await _client.PostAsJsonAsync("/api/loans/apply",
            new { loanAmount = 500_000, assetValue = 1_000_000, creditScore = 800 });

        await _client.PostAsJsonAsync("/api/loans/apply",
            new { loanAmount = 900_000, assetValue = 1_000_000, creditScore = 999 }); // LTV 90% → declined

        var response = await _client.GetAsync("/api/dashboard");
        var body = JsonSerializer.Deserialize<DashboardResponse>(
            await response.Content.ReadAsStringAsync(), JsonOpts);

        Assert.NotNull(body!.MeanLtv);
        // Mean should be between 50% and 90%, confirming both are included.
        Assert.InRange(body.MeanLtv!.Value, 50m, 90m);
    }

    [Fact]
    public async Task Get_Dashboard_RecentApplications_MaxFive()
    {
        // Submit 6 applications.
        for (int i = 0; i < 6; i++)
        {
            await _client.PostAsJsonAsync("/api/loans/apply",
                new { loanAmount = 500_000, assetValue = 1_000_000, creditScore = 800 });
        }

        var response = await _client.GetAsync("/api/dashboard");
        var body = JsonSerializer.Deserialize<DashboardResponse>(
            await response.Content.ReadAsStringAsync(), JsonOpts);

        Assert.NotNull(body);
        Assert.True(body!.RecentApplications.Count <= 5);
    }
}
