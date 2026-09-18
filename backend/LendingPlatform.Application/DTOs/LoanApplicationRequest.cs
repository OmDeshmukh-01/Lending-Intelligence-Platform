namespace LendingPlatform.Application.DTOs;

/// <summary>
/// Incoming request DTO for a loan application submission.
/// Note: creditScore is typed as int so that 750.5 will fail deserialisation
/// at the model-binding layer, producing a 400 before validation even runs.
/// </summary>
public sealed class LoanApplicationRequest
{
    public decimal LoanAmount { get; init; }
    public decimal AssetValue { get; init; }
    public int CreditScore { get; init; }
}
