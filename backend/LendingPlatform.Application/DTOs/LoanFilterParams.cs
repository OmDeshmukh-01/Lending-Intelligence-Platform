using LendingPlatform.Domain.Enums;

namespace LendingPlatform.Application.DTOs;

/// <summary>Query parameters for filtering the applications list.</summary>
public sealed class LoanFilterParams
{
    public LoanDecision? Decision { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
