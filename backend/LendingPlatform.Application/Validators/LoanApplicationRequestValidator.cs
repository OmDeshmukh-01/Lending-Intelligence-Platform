using LendingPlatform.Application.DTOs;

namespace LendingPlatform.Application.Validators;

/// <summary>Validates the raw input for a loan application request.</summary>
public static class LoanApplicationRequestValidator
{
    public static ValidationResult Validate(LoanApplicationRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (request.LoanAmount <= 0)
            AddError(errors, "loanAmount", "Loan amount must be greater than £0.");

        if (request.AssetValue <= 0)
            AddError(errors, "assetValue", "Asset value must be greater than £0.");

        // Credit score bounds: 1–999 inclusive.
        // Non-integer values are caught at deserialisation; this guards 0 and 1000+.
        if (request.CreditScore < 1 || request.CreditScore > 999)
            AddError(errors, "creditScore", "Credit score must be between 1 and 999.");

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors.ToDictionary(k => k.Key, v => v.Value.ToArray()));
    }

    private static void AddError(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.ContainsKey(field)) errors[field] = new List<string>();
        errors[field].Add(message);
    }
}

public sealed class ValidationResult
{
    public bool IsValid { get; private init; }
    public IDictionary<string, string[]> Errors { get; private init; }
        = new Dictionary<string, string[]>();

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failure(IDictionary<string, string[]> errors)
        => new() { IsValid = false, Errors = errors };
}
