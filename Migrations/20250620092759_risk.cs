using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentPortfolioManagement.Migrations
{
    /// <inheritdoc />
    public partial class risk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortfolioRiskAnalyses",
                columns: table => new
                {
                    PortfolioRiskAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskLevel = table.Column<int>(type: "int", nullable: false),
                    AnalysisDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnalysisDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioRiskAnalyses", x => x.PortfolioRiskAnalysisId);
                    table.ForeignKey(
                        name: "FK_PortfolioRiskAnalyses_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskProfile_UserId",
                table: "RiskProfile",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioRiskAnalyses_PortfolioId",
                table: "PortfolioRiskAnalyses",
                column: "PortfolioId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskProfile_Users_UserId",
                table: "RiskProfile",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskProfile_Users_UserId",
                table: "RiskProfile");

            migrationBuilder.DropTable(
                name: "PortfolioRiskAnalyses");

            migrationBuilder.DropIndex(
                name: "IX_RiskProfile_UserId",
                table: "RiskProfile");
        }
    }
}
