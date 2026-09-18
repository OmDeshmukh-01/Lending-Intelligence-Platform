using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using LendingPlatform.Api.Middleware;
using LendingPlatform.Application.Interfaces;
using LendingPlatform.Application.Services;
using LendingPlatform.Infrastructure.Persistence;
using LendingPlatform.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialise enums as strings (e.g., "Approved"/"Declined") for readable API responses.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ── Database ─────────────────────────────────────────────────────────────────────
var dbPath = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=lending.db";

builder.Services.AddDbContext<LendingDbContext>(options =>
    options.UseSqlite(dbPath));

// ── Dependency injection ──────────────────────────────────────────────────────────
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<LoanApplicationService>();

// ── CORS ─────────────────────────────────────────────────────────────────────────
// Open to localhost origins for local development. Restrict in production.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:3000",
            "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.MapControllers();

// ── Auto-migrate on startup ───────────────────────────────────────────────────────
// Applies any pending EF Core migrations so the app runs from a clean clone.
// Skipped in the Test environment (WebApplicationFactory handles schema creation
// via EnsureCreated() which is appropriate for in-memory SQLite test databases).
if (!app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LendingDbContext>();
    db.Database.Migrate();
}

app.Run();

// Required by WebApplicationFactory for integration tests.
public partial class Program { }
