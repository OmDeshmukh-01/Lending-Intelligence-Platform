using LendingPlatform.Domain.Enums;

namespace LendingPlatform.Domain.Entities;

/// <summary>
/// Persistent record of a loan application. Treated as an immutable audit record
/// once created — the decision and rule evaluations are stored at submission time
/// so that future changes to lending rules do not alter historical decisions.
/// </summary>
public class LoanApplication
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Requested loan amount in GBP.</summary>
    public decimal LoanAmount { get; init; }

    /// <summary>Value of the asset the loan is secured against.</summary>
    public decimal AssetValue { get; init; }

    /// <summary>Applicant credit score (1–999).</summary>
    public int CreditScore { get; init; }

    /// <summary>
    /// Loan to Value ratio calculated at submission time with full decimal precision.
    /// Stored at this precision so the historical record matches the decision made.
    /// </summary>
    public decimal Ltv { get; init; }

    public LoanDecision Decision { get; init; }

    public string DecisionReason { get; init; } = string.Empty;

    /// <summary>
    /// JSON-serialised array of RuleResult objects captured at submission time.
    /// Stored as JSON to preserve the exact evaluation even if rule logic changes later.
    /// </summary>
    public string RulesJson { get; init; } = "[]";

    /// <summary>UTC timestamp of application creation.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
