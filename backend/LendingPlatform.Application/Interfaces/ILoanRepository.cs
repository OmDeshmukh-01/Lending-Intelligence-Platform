using LendingPlatform.Application.DTOs;
using LendingPlatform.Domain.Entities;

namespace LendingPlatform.Application.Interfaces;

/// <summary>
/// Persistence contract for loan applications. Defined in the Application layer
/// so that the Application layer does not depend on Infrastructure.
/// The Infrastructure layer provides the concrete implementation.
/// </summary>
public interface ILoanRepository
{
    Task AddAsync(LoanApplication application, CancellationToken ct = default);
    Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<LoanApplication>> GetPagedAsync(LoanFilterParams filter, CancellationToken ct = default);
    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default);
}

/// <summary>
/// Raw aggregate statistics fetched from the database in a single query.
/// Mapped to DashboardResponse in the service layer.
/// </summary>
public sealed class DashboardStats
{
    public int TotalApplications { get; init; }
    public int SuccessfulApplications { get; init; }
    public int DeclinedApplications { get; init; }
    public decimal TotalLoansWritten { get; init; }
        public decimal? MeanLtv { get; init; }
    public IReadOnlyList<LoanAmountRangeDto> LoanAmountDistribution { get; init; } = Array.Empty<LoanAmountRangeDto>();
    public IReadOnlyList<LoanApplication> RecentApplications { get; init; } = Array.Empty<LoanApplication>();
}
