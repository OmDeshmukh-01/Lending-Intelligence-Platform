using LendingPlatform.Domain.Enums;

namespace LendingPlatform.Application.DTOs;

/// <summary>
/// Lightweight DTO used in list views and the dashboard recent-applications panel.
/// Does not include rule evaluations to keep the payload small.
/// </summary>
public sealed class LoanApplicationSummary
{
    public Guid Id { get; init; }
    public decimal LoanAmount { get; init; }
    public decimal AssetValue { get; init; }
    public int CreditScore { get; init; }
    public decimal Ltv { get; init; }
    public LoanDecision Decision { get; init; }
    public DateTime CreatedAt { get; init; }
}
