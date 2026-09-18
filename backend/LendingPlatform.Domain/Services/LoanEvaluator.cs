using LendingPlatform.Domain.Enums;

namespace LendingPlatform.Domain.Services;

/// <summary>
/// Pure, stateless lending decision engine.
///
/// Design rationale: implemented as a static class because the decision function
/// is a pure mapping from inputs to outputs with no side effects and no shared
/// state. This makes every test a direct method call with no setup overhead.
///
/// Dependency rule: this class MUST NOT reference any ASP.NET, EF Core, SQLite,
/// or HTTP types. It belongs to the Domain layer and must remain independently
/// testable.
/// </summary>
public static class LoanEvaluator
{
    // ── Business limits ─────────────────────────────────────────────────────────
    private const decimal MinLoanAmount = 100_000m;
    private const decimal MaxLoanAmount = 1_500_000m;
    private const decimal HighValueThreshold = 1_000_000m;

    // ── High-value thresholds ────────────────────────────────────────────────────
    private const decimal HighValueMaxLtv = 60m;
    private const int HighValueMinScore = 950;

    // ── Standard-loan LTV band boundaries ───────────────────────────────────────
    private const decimal Band1MaxLtv = 60m;   // LTV < 60
    private const decimal Band2MaxLtv = 80m;   // 60 <= LTV < 80
    private const decimal Band3MaxLtv = 90m;   // 80 <= LTV < 90; >= 90 → declined

    // ── Standard-loan minimum credit scores ─────────────────────────────────────
    private const int Band1MinScore = 750;
    private const int Band2MinScore = 800;
    private const int Band3MinScore = 900;

    /// <summary>Calculates LTV and evaluates all applicable business rules.</summary>
    public static EvaluationResult Evaluate(decimal loanAmount, decimal assetValue, int creditScore)
    {
        // LTV computed at full decimal precision — never rounded before rule checks.
        decimal ltv = loanAmount / assetValue * 100m;

        var rules = new List<RuleResult>();

        // ── Rule 1: Loan amount within permitted range ───────────────────────────
        bool loanInRange = loanAmount >= MinLoanAmount && loanAmount <= MaxLoanAmount;
        rules.Add(new RuleResult(
            "Loan amount within permitted range (£100,000 – £1,500,000)",
            loanInRange,
            loanInRange
                ? $"£{loanAmount:N0} is within the permitted range"
                : $"£{loanAmount:N0} is outside the permitted range of £100,000 – £1,500,000"));

        if (!loanInRange)
        {
            return new EvaluationResult(
                LoanDecision.Declined,
                $"Loan amount of £{loanAmount:N0} is outside the permitted range of £100,000 – £1,500,000.",
                rules.AsReadOnly());
        }

        // ── Route: high-value loan (>= £1,000,000) ──────────────────────────────
        if (loanAmount >= HighValueThreshold)
        {
            return EvaluateHighValue(loanAmount, creditScore, ltv, rules);
        }

        // ── Route: standard loan (£100,000 <= amount < £1,000,000) ──────────────
        return EvaluateStandard(creditScore, ltv, rules);
    }

    private static EvaluationResult EvaluateHighValue(
        decimal loanAmount, int creditScore, decimal ltv, List<RuleResult> rules)
    {
        // Both conditions must be satisfied; we evaluate both so the UI can show
        // exactly which (if any) failed.
        bool ltvOk = ltv <= HighValueMaxLtv;
        bool scoreOk = creditScore >= HighValueMinScore;

        rules.Add(new RuleResult(
            $"LTV must be ≤ {HighValueMaxLtv}% for loans of £1,000,000 or more",
            ltvOk,
            ltvOk
                ? $"LTV of {ltv:F2}% is ≤ {HighValueMaxLtv}%"
                : $"LTV of {ltv:F2}% exceeds the {HighValueMaxLtv}% maximum for high-value loans"));

        rules.Add(new RuleResult(
            $"Credit score must be ≥ {HighValueMinScore} for loans of £1,000,000 or more",
            scoreOk,
            scoreOk
                ? $"Credit score of {creditScore} meets the {HighValueMinScore} requirement"
                : $"Credit score of {creditScore} is below the {HighValueMinScore} requirement for high-value loans"));

        bool approved = ltvOk && scoreOk;
        return new EvaluationResult(
            approved ? LoanDecision.Approved : LoanDecision.Declined,
            approved
                ? "Application meets the applicable lending criteria."
                : BuildHighValueDeclineReason(ltvOk, scoreOk, ltv, creditScore),
            rules.AsReadOnly());
    }

    private static EvaluationResult EvaluateStandard(
        int creditScore, decimal ltv, List<RuleResult> rules)
    {
        // Band 4: LTV >= 90% — declined without credit score check.
        if (ltv >= Band3MaxLtv)
        {
            rules.Add(new RuleResult(
                "LTV must be below 90% (Band 4: automatically declined)",
                false,
                $"LTV of {ltv:F2}% is at or above 90%; this application is automatically declined regardless of credit score"));

            return new EvaluationResult(
                LoanDecision.Declined,
                $"LTV of {ltv:F2}% is at or above 90%. Applications with LTV of 90% or above are automatically declined.",
                rules.AsReadOnly());
        }

        // Determine which band applies and the required score.
        // Important boundaries:
        //   LTV < 60%  → Band 1  (score >= 750)
        //   LTV = 60%  → Band 2  (score >= 800)  [60 is NOT in Band 1]
        //   LTV = 80%  → Band 3  (score >= 900)  [80 is NOT in Band 2]
        int requiredScore;
        string bandName;

        if (ltv < Band1MaxLtv)
        {
            requiredScore = Band1MinScore;
            bandName = $"Band 1 (LTV < {Band1MaxLtv}%)";
        }
        else if (ltv < Band2MaxLtv)
        {
            requiredScore = Band2MinScore;
            bandName = $"Band 2 ({Band1MaxLtv}% ≤ LTV < {Band2MaxLtv}%)";
        }
        else
        {
            requiredScore = Band3MinScore;
            bandName = $"Band 3 ({Band2MaxLtv}% ≤ LTV < {Band3MaxLtv}%)";
        }

        rules.Add(new RuleResult(
            $"LTV band determination",
            true,
            $"LTV of {ltv:F2}% falls into {bandName}; minimum credit score required: {requiredScore}"));

        bool scoreOk = creditScore >= requiredScore;
        rules.Add(new RuleResult(
            $"Credit score meets {bandName} requirement (≥ {requiredScore})",
            scoreOk,
            scoreOk
                ? $"Credit score of {creditScore} meets the requirement of ≥ {requiredScore}"
                : $"Credit score of {creditScore} is below the requirement of ≥ {requiredScore} for {bandName}"));

        return new EvaluationResult(
            scoreOk ? LoanDecision.Approved : LoanDecision.Declined,
            scoreOk
                ? "Application meets the applicable lending criteria."
                : $"Credit score of {creditScore} does not meet the minimum requirement of {requiredScore} for {bandName} (LTV: {ltv:F2}%).",
            rules.AsReadOnly());
    }

    private static string BuildHighValueDeclineReason(bool ltvOk, bool scoreOk, decimal ltv, int creditScore)
    {
        if (!ltvOk && !scoreOk)
            return $"LTV of {ltv:F2}% exceeds the 60% maximum and credit score of {creditScore} is below the 950 minimum for loans of £1,000,000 or more.";
        if (!ltvOk)
            return $"LTV of {ltv:F2}% exceeds the 60% maximum for loans of £1,000,000 or more.";
        return $"Credit score of {creditScore} is below the 950 minimum for loans of £1,000,000 or more.";
    }
}
