using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LendingPlatform.Application.DTOs;
using LendingPlatform.Domain.Enums;
using Xunit;

namespace LendingPlatform.Api.Tests.Integration;

public sealed class LoansControllerTests : IClassFixture<LendingWebAppFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public LoansControllerTests(LendingWebAppFactory factory)
    {
        factory.EnsureDb();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Apply_ValidApprovedApplication_Returns200WithApproved()
    {
        var request = new { loanAmount = 500_000, assetValue = 1_000_000, creditScore = 800 };
        var response = await _client.PostAsJsonAsync("/api/loans/apply", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<LoanApplicationResponse>(json, JsonOpts);

        Assert.NotNull(body);
        Assert.Equal(LoanDecision.Approved, body!.Decision);
        Assert.Equal(500_000m, body.LoanAmount);
        Assert.Equal(50.00m, body.Ltv);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.NotEmpty(body.Rules);
    }

    [Fact]
    public async Task Post_Apply_ValidDeclinedApplication_Returns200WithDeclined()
    {
        // LTV 70%, score 750 (below Band 2 requirement of 800) → Declined
        var request = new { loanAmount = 700_000, assetValue = 1_000_000, creditScore = 750 };
        var response = await _client.PostAsJsonAsync("/api/loans/apply", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonSerializer.Deserialize<LoanApplicationResponse>(
            await response.Content.ReadAsStringAsync(), JsonOpts);

        Assert.NotNull(body);
        Assert.Equal(LoanDecision.Declined, body!.Decision);
        // Score rule should be marked as failed
        Assert.True(body.Rules.Any(r => !r.Passed));
    }

    [Fact]
    public async Task Post_Apply_NegativeLoanAmount_Returns400()
    {
        var request = new { loanAmount = -500_000, assetValue = 1_000_000, creditScore = 800 };
        var response = await _client.PostAsJsonAsync("/api/loans/apply", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Apply_ZeroAssetValue_Returns400()
    {
        var request = new { loanAmount = 500_000, assetValue = 0, creditScore = 800 };
        var response = await _client.PostAsJsonAsync("/api/loans/apply", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Apply_InvalidCreditScore_Returns400()
    {
        var request = new { loanAmount = 500_000, assetValue = 1_000_000, creditScore = 1000 };
        var response = await _client.PostAsJsonAsync("/api/loans/apply", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_ById_ExistingApplication_Returns200()
    {
        // First submit an application.
        var applyRequest = new { loanAmount = 500_000, assetValue = 1_000_000, creditScore = 800 };
        var applyResponse = await _client.PostAsJsonAsync("/api/loans/apply", applyRequest);
        var submitted = JsonSerializer.Deserialize<LoanApplicationResponse>(
            await applyResponse.Content.ReadAsStringAsync(), JsonOpts);

        var response = await _client.GetAsync($"/api/loans/{submitted!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonSerializer.Deserialize<LoanApplicationResponse>(
            await response.Content.ReadAsStringAsync(), JsonOpts);
        Assert.NotNull(body);
        Assert.Equal(submitted.Id, body!.Id);
        Assert.NotEmpty(body.Rules);
    }

    [Fact]
    public async Task Get_ById_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/loans/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Loans_ReturnsPagedResults()
    {
        // Submit a loan first so we have data.
        await _client.PostAsJsonAsync("/api/loans/apply",
            new { loanAmount = 500_000, assetValue = 1_000_000, creditScore = 800 });

        var response = await _client.GetAsync("/api/loans");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonSerializer.Deserialize<PagedResult<LoanApplicationSummary>>(
            await response.Content.ReadAsStringAsync(), JsonOpts);
        Assert.NotNull(body);
        Assert.True(body!.TotalCount >= 1);
    }

    [Fact]
    public async Task Get_Loans_FilterByDecision_ReturnsFilteredResults()
    {
        // Submit an approved application.
        await _client.PostAsJsonAsync("/api/loans/apply",
            new { loanAmount = 500_000, assetValue = 1_000_000, creditScore = 800 });

        var response = await _client.GetAsync("/api/loans?decision=Approved");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonSerializer.Deserialize<PagedResult<LoanApplicationSummary>>(
            await response.Content.ReadAsStringAsync(), JsonOpts);
        Assert.NotNull(body);
        Assert.True(body!.Items.All(x => x.Decision == LoanDecision.Approved));
    }

    [Fact]
    public async Task Get_Loans_InvalidDecisionFilter_Returns400()
    {
        var response = await _client.GetAsync("/api/loans?decision=InvalidValue");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
