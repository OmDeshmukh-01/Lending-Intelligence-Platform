using Microsoft.EntityFrameworkCore;
using LendingPlatform.Domain.Entities;

namespace LendingPlatform.Infrastructure.Persistence;

public sealed class LendingDbContext : DbContext
{
    public LendingDbContext(DbContextOptions<LendingDbContext> options) : base(options) { }

    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LendingDbContext).Assembly);
    }
}
