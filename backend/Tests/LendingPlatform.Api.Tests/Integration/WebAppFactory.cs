using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LendingPlatform.Infrastructure.Persistence;

namespace LendingPlatform.Api.Tests.Integration;

/// <summary>Custom WebApplicationFactory using an in-memory SQLite database.</summary>
public sealed class LendingWebAppFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid():N}";
    private readonly string _connectionString;
    
    // Persistent keeper connection — holds the in-memory database alive.
    private SqliteConnection? _keepAliveConnection;

    public LendingWebAppFactory()
    {
        _connectionString = $"Data Source={_dbName};Mode=Memory;Cache=Shared";
        
        // Open the keeper connection immediately to keep the in-memory DB alive.
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext registration.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<LendingDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Register DbContext to use our named in-memory SQLite database.
            services.AddDbContext<LendingDbContext>(options =>
                options.UseSqlite(_connectionString));
        });
    }

    /// <summary>
    /// Creates the schema in the in-memory database. Call before making HTTP requests.
    /// </summary>
    public void EnsureDb()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LendingDbContext>();
        db.Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _keepAliveConnection?.Close();
            _keepAliveConnection?.Dispose();
            _keepAliveConnection = null;
        }
        base.Dispose(disposing);
    }
}
