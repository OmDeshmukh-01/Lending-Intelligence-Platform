using LendingPlatform.Domain.Enums;
using LendingPlatform.Domain.Services;
using Xunit;

namespace LendingPlatform.Domain.Tests;

/// <summary>
/// Comprehensive boundary tests for LoanEvaluator.
/// These tests have zero dependencies on HTTP, EF Core, SQLite, or React.
/// They are the primary proof that the business rules are correctly implemented.
/// Each boundary from the specification is tested explicitly.
/// </summary>
public sealed class LoanEvaluatorBoundaryTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // LOAN AMOUNT BOUNDARIES
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void LoanAmount_99999_IsDeclined()
    {
        // £99,999 is below the £100,000 minimum.
        var result = LoanEvaluator.Evaluate(99_999m, 200_000m, 800);
        Assert.Equal(LoanDecision.Declined, result.Decision);
        Assert.False(result.Rules[0].Passed); // loan range rule fails
    }

    [Fact]
    public void LoanAmount_100000_IsAllowedToStandardPath()
    {
        // Spec: "£100,000 is allowed to proceed." Boundary is inclusive.
        var result = LoanEvaluator.Evaluate(100_000m, 200_000m, 800);
        Assert.True(result.Rules[0].Passed); // loan range rule passes
    }

    [Fact]
    public void LoanAmount_100001_IsAllowedToStandardPath()
    {
        var result = LoanEvaluator.Evaluate(100_001m, 200_000m, 800);
        Assert.True(result.Rules[0].Passed);
    }

    [Fact]
    public void LoanAmount_999999_GoesToStandardPath()
    {
        // Just below £1M threshold — should use standard (not high-value) rules.
        // LTV = 999999/2000000*100 = 49.99995% (Band 1). Score 800 >= 750 → Approved.
        var result = LoanEvaluator.Evaluate(999_999m, 2_000_000m, 800);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void LoanAmount_1000000_GoesToHighValuePath()
    {
        // Spec: "Exactly £1,000,000 belongs to this high-value-loan rule."
        // LTV = 1000000/2000000*100 = 50% (<= 60%), score 950 >= 950 → Approved.
        var result = LoanEvaluator.Evaluate(1_000_000m, 2_000_000m, 950);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void LoanAmount_1000000_WithScore949_IsDeclined()
    {
        // High-value path, score just below 950.
        var result = LoanEvaluator.Evaluate(1_000_000m, 2_000_000m, 949);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void LoanAmount_1000001_GoesToHighValuePath()
    {
        var result = LoanEvaluator.Evaluate(1_000_001m, 2_000_000m, 950);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void LoanAmount_1500000_IsAllowed()
    {
        // Spec: "£1,500,000 is allowed to proceed." Boundary is inclusive.
        // LTV = 1500000/2500000*100 = 60% exactly → high-value path, LTV OK.
        var result = LoanEvaluator.Evaluate(1_500_000m, 2_500_000m, 950);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void LoanAmount_1500001_IsDeclined()
    {
        var result = LoanEvaluator.Evaluate(1_500_001m, 2_500_000m, 999);
        Assert.Equal(LoanDecision.Declined, result.Decision);
        Assert.False(result.Rules[0].Passed);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LTV BAND BOUNDARIES — STANDARD LOANS (£100k–£999,999)
    // ═══════════════════════════════════════════════════════════════════════════

    // Helper: creates an asset value that produces exactly the target LTV
    // for a £500,000 loan. AssetValue = LoanAmount / (ltv/100).
    private static decimal AssetForLtv(decimal ltv) => 500_000m / (ltv / 100m);

    [Fact]
    public void Ltv_5999_Band1_Score750_IsApproved()
    {
        // LTV = 59.99% → Band 1 (LTV < 60%), requires score >= 750.
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(59.99m), 750);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void Ltv_5999_Band1_Score749_IsDeclined()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(59.99m), 749);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Ltv_6000_IsBand2_Score800_IsApproved()
    {
        // Spec: "60.00% belongs to the second band" (requires score >= 800, not 750).
        // This is the most important boundary — a score of 750 must be declined here.
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(60.00m), 800);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void Ltv_6000_IsBand2_Score799_IsDeclined()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(60.00m), 799);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Ltv_6000_IsBand2_Score750_IsDeclined()
    {
        // Critical: 60% is NOT in Band 1. Score 750 is not sufficient at 60%.
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(60.00m), 750);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Ltv_6001_IsBand2_Score800_IsApproved()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(60.01m), 800);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void Ltv_7999_Band2_Score800_IsApproved()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(79.99m), 800);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void Ltv_7999_Band2_Score799_IsDeclined()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(79.99m), 799);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Ltv_8000_IsBand3_Score900_IsApproved()
    {
        // 80% belongs to Band 3 (requires score >= 900), NOT Band 2.
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(80.00m), 900);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void Ltv_8000_IsBand3_Score899_IsDeclined()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(80.00m), 899);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Ltv_8000_IsBand3_Score800_IsDeclined()
    {
        // 80% is NOT in Band 2. Score 800 is not sufficient.
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(80.00m), 800);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Ltv_8001_IsBand3_Score900_IsApproved()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(80.01m), 900);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void Ltv_8999_Band3_Score900_IsApproved()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(89.99m), 900);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void Ltv_8999_Band3_Score899_IsDeclined()
    {
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(89.99m), 899);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Ltv_9000_IsAutomaticallyDeclined_Regardless_Of_Score()
    {
        // Spec: "LTV >= 90% → Declined regardless of credit score."
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(90.00m), 999);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Ltv_Above100_IsDeclined()
    {
        // LTV > 100% — falls into >= 90% band automatically.
        var result = LoanEvaluator.Evaluate(500_000m, 300_000m, 999);
        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LTV PRECISION — ensure no premature rounding affects rule evaluation
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Ltv_60004_Percent_RoundsTo6000_But_StillInBand2()
    {
        // LoanAmount = 600,040, AssetValue = 1,000,000 → LTV = 60.004%
        // Spec example: "An actual LTV of 60.004% must not be rounded to 60.00%
        // before applying the rules." It belongs to Band 2 (requires score >= 800).
        var result = LoanEvaluator.Evaluate(600_040m, 1_000_000m, 800);
        Assert.Equal(LoanDecision.Approved, result.Decision);

        // Same LTV with score 750 must be declined (Band 2, not Band 1).
        var result2 = LoanEvaluator.Evaluate(600_040m, 1_000_000m, 750);
        Assert.Equal(LoanDecision.Declined, result2.Decision);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREDIT SCORE BOUNDARIES PER BAND
    // ═══════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(749, false)]  // Below Band 1 minimum
    [InlineData(750, true)]   // At Band 1 minimum — exactly allowed
    [InlineData(751, true)]   // Above Band 1 minimum
    public void Band1_CreditScore_Boundaries(int score, bool expectedApproved)
    {
        // LTV 50% → Band 1
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(50m), score);
        Assert.Equal(expectedApproved ? LoanDecision.Approved : LoanDecision.Declined, result.Decision);
    }

    [Theory]
    [InlineData(799, false)]
    [InlineData(800, true)]
    [InlineData(801, true)]
    public void Band2_CreditScore_Boundaries(int score, bool expectedApproved)
    {
        // LTV 70% → Band 2
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(70m), score);
        Assert.Equal(expectedApproved ? LoanDecision.Approved : LoanDecision.Declined, result.Decision);
    }

    [Theory]
    [InlineData(899, false)]
    [InlineData(900, true)]
    [InlineData(901, true)]
    public void Band3_CreditScore_Boundaries(int score, bool expectedApproved)
    {
        // LTV 85% → Band 3
        var result = LoanEvaluator.Evaluate(500_000m, AssetForLtv(85m), score);
        Assert.Equal(expectedApproved ? LoanDecision.Approved : LoanDecision.Declined, result.Decision);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HIGH-VALUE LOAN COMBINATIONS (>= £1M)
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void HighValue_LtvBelow60_Score950_IsApproved()
    {
        var result = LoanEvaluator.Evaluate(1_000_000m, 2_000_000m, 950); // LTV 50%
        Assert.Equal(LoanDecision.Approved, result.Decision);
        Assert.True(result.Rules.All(r => r.Passed));
    }

    [Fact]
    public void HighValue_LtvExactly60_Score950_IsApproved()
    {
        // LTV = 1000000/1666666.67*100 ≈ 60.0% — boundary is inclusive (<= 60%).
        var result = LoanEvaluator.Evaluate(1_000_000m, 1_000_000m / 0.6m, 950);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void HighValue_LtvAbove60_Score950_IsDeclined()
    {
        // LTV = 1000000/1500000*100 = 66.67%
        var result = LoanEvaluator.Evaluate(1_000_000m, 1_500_000m, 950);
        Assert.Equal(LoanDecision.Declined, result.Decision);
        // LTV rule should be the failing rule
        var ltvRule = result.Rules.FirstOrDefault(r => r.Name.Contains("LTV"));
        Assert.NotNull(ltvRule);
        Assert.False(ltvRule!.Passed);
    }

    [Fact]
    public void HighValue_LtvBelow60_Score949_IsDeclined()
    {
        var result = LoanEvaluator.Evaluate(1_000_000m, 2_000_000m, 949);
        Assert.Equal(LoanDecision.Declined, result.Decision);
        var scoreRule = result.Rules.FirstOrDefault(r => r.Name.Contains("950"));
        Assert.NotNull(scoreRule);
        Assert.False(scoreRule!.Passed);
    }

    [Theory]
    [InlineData(949, false)]
    [InlineData(950, true)]
    [InlineData(951, true)]
    public void HighValue_CreditScore_Boundaries(int score, bool expectedApproved)
    {
        var result = LoanEvaluator.Evaluate(1_200_000m, 2_000_000m, score); // LTV 60%
        Assert.Equal(expectedApproved ? LoanDecision.Approved : LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void HighValue_BothConditionsFail_IsDeclined_WithBothRulesFailed()
    {
        // LTV > 60% AND score < 950 — both rules should be marked failed.
        var result = LoanEvaluator.Evaluate(1_000_000m, 1_500_000m, 900); // LTV ~66.7%
        Assert.Equal(LoanDecision.Declined, result.Decision);
        var ltvRule = result.Rules.FirstOrDefault(r => r.Name.Contains("LTV"));
        var scoreRule = result.Rules.FirstOrDefault(r => r.Name.Contains("950"));
        Assert.NotNull(ltvRule);
        Assert.NotNull(scoreRule);
        Assert.False(ltvRule!.Passed);
        Assert.False(scoreRule!.Passed);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LTV CALCULATION CORRECTNESS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Ltv_Calculation_50Percent_Correct()
    {
        // LTV = 500000 / 1000000 * 100 = 50%
        // Verify: result is Approved (correct path), and LTV appears somewhere in rule details.
        var result = LoanEvaluator.Evaluate(500_000m, 1_000_000m, 800);
        Assert.Equal(LoanDecision.Approved, result.Decision);
        // The band determination rule should mention the LTV value.
        var allDetails = string.Join(" | ", result.Rules.Select(r => r.Detail));
        Assert.Contains("50.00", allDetails,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ltv_Calculation_ExactExample_FromSpec()
    {
        // Spec example: Loan = £500k, Asset = £1M → LTV = 50%
        var result = LoanEvaluator.Evaluate(500_000m, 1_000_000m, 800);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RULE RESULT CORRECTNESS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Rules_AreOnlyRelevantToActualDecisionPath()
    {
        // An application declined at the loan amount check should only have 1 rule.
        var result = LoanEvaluator.Evaluate(50_000m, 100_000m, 999);
        Assert.Single(result.Rules);
    }

    [Fact]
    public void Rules_HighValuePath_AlwaysHasThreeRules()
    {
        // Range check + LTV check + score check.
        var result = LoanEvaluator.Evaluate(1_000_000m, 2_000_000m, 950);
        Assert.Equal(3, result.Rules.Count);
    }

    [Fact]
    public void Rules_StandardPath_Band4_DeclineHasTwoRules()
    {
        // Range check + LTV band 4 decline rule.
        var result = LoanEvaluator.Evaluate(500_000m, 400_000m, 999); // LTV 125% → Band 4
        Assert.Equal(2, result.Rules.Count);
    }

    [Fact]
    public void Rules_StandardPath_Bands1to3_HasThreeRules()
    {
        // Range check + band determination + score check.
        var result = LoanEvaluator.Evaluate(500_000m, 1_000_000m, 800);
        Assert.Equal(3, result.Rules.Count);
    }
}
