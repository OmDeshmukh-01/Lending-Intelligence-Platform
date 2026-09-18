using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoanApplications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LoanAmount = table.Column<string>(type: "TEXT", nullable: false),
                    AssetValue = table.Column<string>(type: "TEXT", nullable: false),
                    CreditScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Ltv = table.Column<string>(type: "TEXT", nullable: false),
                    Decision = table.Column<int>(type: "INTEGER", nullable: false),
                    DecisionReason = table.Column<string>(type: "TEXT", nullable: false),
                    RulesJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_CreatedAt",
                table: "LoanApplications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_Decision",
                table: "LoanApplications",
                column: "Decision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanApplications");
        }
    }
}
