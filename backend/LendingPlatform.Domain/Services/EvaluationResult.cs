using LendingPlatform.Domain.Enums;

namespace LendingPlatform.Domain.Services;

/// <summary>
/// The complete output of the lending decision engine for a single application.
/// </summary>
public sealed record EvaluationResult(
    LoanDecision Decision,
    string Reason,
    IReadOnlyList<RuleResult> Rules);
