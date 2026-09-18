using LendingPlatform.Application.DTOs;
using LendingPlatform.Application.Validators;
using Xunit;

namespace LendingPlatform.Domain.Tests;

/// <summary>
/// Tests for input validation — separate from business rule tests.
/// Validates that structurally invalid inputs are rejected before business rules run.
/// </summary>
public sealed class LoanEvaluatorValidationTests
{
    private static ValidationResult Validate(decimal loan, decimal asset, int score) =>
        LoanApplicationRequestValidator.Validate(new LoanApplicationRequest
        {
            LoanAmount = loan,
            AssetValue = asset,
            CreditScore = score
        });

    [Fact]
    public void ValidInput_ReturnsSuccess()
    {
        var result = Validate(500_000m, 1_000_000m, 750);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ZeroLoanAmount_IsValidationError()
    {
        var result = Validate(0m, 1_000_000m, 750);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("loanAmount"));
    }

    [Fact]
    public void NegativeLoanAmount_IsValidationError()
    {
        var result = Validate(-500_000m, 1_000_000m, 750);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("loanAmount"));
    }

    [Fact]
    public void ZeroAssetValue_IsValidationError()
    {
        // Zero asset value would cause division by zero in LTV calculation.
        var result = Validate(500_000m, 0m, 750);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("assetValue"));
    }

    [Fact]
    public void NegativeAssetValue_IsValidationError()
    {
        var result = Validate(500_000m, -1_000_000m, 750);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("assetValue"));
    }

    [Fact]
    public void CreditScore_0_IsValidationError()
    {
        var result = Validate(500_000m, 1_000_000m, 0);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("creditScore"));
    }

    [Fact]
    public void CreditScore_1_IsValid()
    {
        var result = Validate(500_000m, 1_000_000m, 1);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreditScore_999_IsValid()
    {
        var result = Validate(500_000m, 1_000_000m, 999);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreditScore_1000_IsValidationError()
    {
        var result = Validate(500_000m, 1_000_000m, 1000);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("creditScore"));
    }

    [Fact]
    public void CreditScore_Negative_IsValidationError()
    {
        var result = Validate(500_000m, 1_000_000m, -10);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("creditScore"));
    }

    [Fact]
    public void MultipleInvalidFields_ReturnsAllErrors()
    {
        var result = Validate(-1m, -1m, 0);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("loanAmount"));
        Assert.True(result.Errors.ContainsKey("assetValue"));
        Assert.True(result.Errors.ContainsKey("creditScore"));
    }

    [Fact]
    public void LoanAmount_SmallButPositive_IsValid_Input_ButDeclinedByBusinessRules()
    {
        // £50,000 is valid input (passes validation) but will be declined by business rules.
        // This test confirms the separation between validation and business rules.
        var validationResult = Validate(50_000m, 1_000_000m, 750);
        Assert.True(validationResult.IsValid);
        // Business rule decline is tested in LoanEvaluatorBoundaryTests.
    }
}
