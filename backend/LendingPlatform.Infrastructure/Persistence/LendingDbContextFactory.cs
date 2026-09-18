using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LendingPlatform.Infrastructure.Persistence;

/// <summary>
/// Design-time factory required by dotnet-ef migrations tool.
/// Only used during development; not invoked at runtime.
/// </summary>
public sealed class LendingDbContextFactory : IDesignTimeDbContextFactory<LendingDbContext>
{
    public LendingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LendingDbContext>();
        optionsBuilder.UseSqlite("Data Source=lending.db");
        return new LendingDbContext(optionsBuilder.Options);
    }
}
