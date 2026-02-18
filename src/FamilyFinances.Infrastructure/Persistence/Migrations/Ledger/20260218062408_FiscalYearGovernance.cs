using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinances.Infrastructure.Persistence.Migrations.Ledger
{
    /// <inheritdoc />
    public partial class FiscalYearGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountYearSnapshots",
                columns: table => new
                {
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClosingBalanceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    ComputedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountYearSnapshots", x => new { x.Year, x.AccountId });
                    table.ForeignKey(
                        name: "FK_AccountYearSnapshots_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiscalYearClosures",
                columns: table => new
                {
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    ReopenedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReopenedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalYearClosures", x => x.Year);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BookedOn",
                table: "Transactions",
                column: "BookedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BookedOn_CreatedAt",
                table: "Transactions",
                columns: new[] { "BookedOn", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountYearSnapshots_AccountId_Year",
                table: "AccountYearSnapshots",
                columns: new[] { "AccountId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYearClosures_IsClosed_Year",
                table: "FiscalYearClosures",
                columns: new[] { "IsClosed", "Year" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountYearSnapshots");

            migrationBuilder.DropTable(
                name: "FiscalYearClosures");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BookedOn",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BookedOn_CreatedAt",
                table: "Transactions");
        }
    }
}
