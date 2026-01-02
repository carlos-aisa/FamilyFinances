using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinances.Infrastructure.Persistence.Migrations.Ledger
{
    /// <inheritdoc />
    public partial class InitialLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Nature = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClosedOn = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedOn = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionSplits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AmountCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Memo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionSplits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionSplits_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLinks_SourceTransactionId",
                table: "TransactionLinks",
                column: "SourceTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLinks_SourceTransactionId_TargetTransactionId_Type",
                table: "TransactionLinks",
                columns: new[] { "SourceTransactionId", "TargetTransactionId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLinks_TargetTransactionId",
                table: "TransactionLinks",
                column: "TargetTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSplits_AccountId",
                table: "TransactionSplits",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSplits_TransactionId",
                table: "TransactionSplits",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "TransactionLinks");

            migrationBuilder.DropTable(
                name: "TransactionSplits");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
