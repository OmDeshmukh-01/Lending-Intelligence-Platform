using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LendingPlatform.Domain.Entities;

namespace LendingPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for LoanApplication.
///
/// Decimal-to-TEXT design rationale:
/// SQLite has no native DECIMAL type. Storing monetary values as REAL (float)
/// would introduce floating-point rounding errors that could corrupt the stored
/// LTV and loan amounts. We use TEXT columns with value converters so that the
/// domain model uses decimal throughout and only the SQLite layer uses strings.
/// </summary>
public sealed class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("LoanApplications");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion<string>();

        // Monetary values stored as TEXT to preserve full decimal precision in SQLite.
        builder.Property(x => x.LoanAmount)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.AssetValue)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Ltv)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreditScore).IsRequired();

        // Decision stored as integer (enum value: 0=Approved, 1=Declined).
        builder.Property(x => x.Decision).IsRequired();

        builder.Property(x => x.DecisionReason).IsRequired();

        // Rule evaluations stored as JSON snapshot for audit immutability.
        builder.Property(x => x.RulesJson)
            .HasColumnName("RulesJson")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasConversion<string>()
            .IsRequired();

        // Index to support common filters efficiently.
        builder.HasIndex(x => x.Decision);
        builder.HasIndex(x => x.CreatedAt);
    }
}
