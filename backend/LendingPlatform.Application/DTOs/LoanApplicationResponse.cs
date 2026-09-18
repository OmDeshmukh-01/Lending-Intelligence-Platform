using LendingPlatform.Domain.Enums;

namespace LendingPlatform.Application.DTOs;

/// <summary>
/// Full response DTO returned after submitting an application and when fetching
/// a single application by ID. Contains everything the frontend needs to display
/// the decision and rule evaluations.
/// </summary>
public sealed class LoanApplicationResponse
{
    public Guid Id { get; init; }
    public decimal LoanAmount { get; init; }
    public decimal AssetValue { get; init; }
    public int CreditScore { get; init; }

    /// <summary>LTV rounded to 2 decimal places for display purposes only.</summary>
    public decimal Ltv { get; init; }

    public LoanDecision Decision { get; init; }
    public string DecisionReason { get; init; } = string.Empty;
    public IReadOnlyList<RuleResultDto> Rules { get; init; } = Array.Empty<RuleResultDto>();
    public DateTime CreatedAt { get; init; }
}
