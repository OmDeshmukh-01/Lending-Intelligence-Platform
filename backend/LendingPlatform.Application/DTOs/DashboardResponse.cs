namespace LendingPlatform.Application.DTOs;

public sealed class LoanAmountRangeDto
{
    public string RangeLabel { get; init; } = "";
    public int Count { get; init; }
}

public sealed class DashboardResponse
{
    public int TotalApplications { get; init; }
    public int SuccessfulApplications { get; init; }
    public int DeclinedApplications { get; init; }

    /// <summary>
    /// Sum of loan amounts for SUCCESSFUL applications only.
    /// Declined applications do not contribute to this figure.
    /// </summary>
    public decimal TotalLoansWritten { get; init; }

    /// <summary>
    /// Mean LTV across ALL applications (both approved and declined).
    /// Null when there are no applications.
    /// </summary>
    public decimal? MeanLtv { get; init; }

    /// <summary>Five most recent applications, ordered newest first.</summary>
    public IReadOnlyList<LoanAmountRangeDto> LoanAmountDistribution { get; init; } = Array.Empty<LoanAmountRangeDto>();
    public IReadOnlyList<LoanApplicationSummary> RecentApplications { get; init; }
        = Array.Empty<LoanApplicationSummary>();
}
