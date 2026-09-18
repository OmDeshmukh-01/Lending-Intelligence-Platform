namespace LendingPlatform.Domain.Services;

/// <summary>
/// Immutable record of a single business rule evaluation.
/// Exposed to the frontend so applicants can understand why a decision was made.
/// </summary>
public sealed record RuleResult(string Name, bool Passed, string Detail);
