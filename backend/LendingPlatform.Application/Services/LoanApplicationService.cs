using System.Text.Json;
using LendingPlatform.Application.DTOs;
using LendingPlatform.Application.Interfaces;
using LendingPlatform.Application.Validators;
using LendingPlatform.Domain.Entities;
using LendingPlatform.Domain.Services;

namespace LendingPlatform.Application.Services;

/// <summary>Orchestrates the loan application use case.</summary>
public sealed class LoanApplicationService
{
    private readonly ILoanRepository _repository;

    public LoanApplicationService(ILoanRepository repository)
    {
        _repository = repository;
    }

    public async Task<(LoanApplicationResponse? Response, ValidationErrorResponse? ValidationError)>
        ApplyAsync(LoanApplicationRequest request, CancellationToken ct = default)
    {
        var validation = LoanApplicationRequestValidator.Validate(request);
        if (!validation.IsValid)
        {
            return (null, new ValidationErrorResponse { Errors = validation.Errors });
        }

        var result = LoanEvaluator.Evaluate(request.LoanAmount, request.AssetValue, request.CreditScore);

        decimal ltvFull = request.LoanAmount / request.AssetValue * 100m;

        var rulesJson = JsonSerializer.Serialize(result.Rules.Select(r => new
        {
            name = r.Name,
            passed = r.Passed,
            detail = r.Detail
        }));

        var application = new LoanApplication
        {
            LoanAmount = request.LoanAmount,
            AssetValue = request.AssetValue,
            CreditScore = request.CreditScore,
            Ltv = ltvFull,
            Decision = result.Decision,
            DecisionReason = result.Reason,
            RulesJson = rulesJson,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(application, ct);

        return (MapToResponse(application, result), null);
    }

    public async Task<LoanApplicationResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var application = await _repository.GetByIdAsync(id, ct);
        if (application is null) return null;

        var rules = DeserialiseRules(application.RulesJson);
        return new LoanApplicationResponse
        {
            Id = application.Id,
            LoanAmount = application.LoanAmount,
            AssetValue = application.AssetValue,
            CreditScore = application.CreditScore,
            Ltv = Math.Round(application.Ltv, 2),
            Decision = application.Decision,
            DecisionReason = application.DecisionReason,
            Rules = rules,
            CreatedAt = application.CreatedAt
        };
    }

    public async Task<PagedResult<LoanApplicationSummary>> GetPagedAsync(
        LoanFilterParams filter, CancellationToken ct = default)
    {
        // Guard page size so callers cannot request unbounded data.
        var safeFilter = new LoanFilterParams
        {
            Decision = filter.Decision,
            From = filter.From,
            To = filter.To,
            Page = filter.Page,
            PageSize = Math.Min(filter.PageSize, 100)
        };
        var paged = await _repository.GetPagedAsync(safeFilter, ct);

        return new PagedResult<LoanApplicationSummary>
        {
            Items = paged.Items.Select(MapToSummary).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages
        };
    }

    public async Task<DashboardResponse> GetDashboardAsync(CancellationToken ct = default)
    {
        var stats = await _repository.GetDashboardStatsAsync(ct);

        return new DashboardResponse
        {
            TotalApplications = stats.TotalApplications,
            SuccessfulApplications = stats.SuccessfulApplications,
            DeclinedApplications = stats.DeclinedApplications,
            TotalLoansWritten = stats.TotalLoansWritten,
            MeanLtv = stats.MeanLtv.HasValue ? Math.Round(stats.MeanLtv.Value, 2) : null,
            LoanAmountDistribution = stats.LoanAmountDistribution,
            RecentApplications = stats.RecentApplications.Select(MapToSummary).ToList()
        };
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static LoanApplicationResponse MapToResponse(
        LoanApplication application, EvaluationResult result)
    {
        return new LoanApplicationResponse
        {
            Id = application.Id,
            LoanAmount = application.LoanAmount,
            AssetValue = application.AssetValue,
            CreditScore = application.CreditScore,
            Ltv = Math.Round(application.Ltv, 2),
            Decision = application.Decision,
            DecisionReason = application.DecisionReason,
            Rules = result.Rules.Select(r => new RuleResultDto
            {
                Name = r.Name,
                Passed = r.Passed,
                Detail = r.Detail
            }).ToList(),
            CreatedAt = application.CreatedAt
        };
    }

    private static LoanApplicationSummary MapToSummary(LoanApplication application)
    {
        return new LoanApplicationSummary
        {
            Id = application.Id,
            LoanAmount = application.LoanAmount,
            AssetValue = application.AssetValue,
            CreditScore = application.CreditScore,
            Ltv = Math.Round(application.Ltv, 2),
            Decision = application.Decision,
            CreatedAt = application.CreatedAt
        };
    }

    private static IReadOnlyList<RuleResultDto> DeserialiseRules(string json)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var rules = JsonSerializer.Deserialize<List<RuleResultDto>>(json, options);
            return rules?.AsReadOnly() ?? (IReadOnlyList<RuleResultDto>)Array.Empty<RuleResultDto>();
        }
        catch
        {
            return Array.Empty<RuleResultDto>();
        }
    }
}
