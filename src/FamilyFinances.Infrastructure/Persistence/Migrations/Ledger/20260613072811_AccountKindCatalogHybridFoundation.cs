using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinances.Infrastructure.Persistence.Migrations.Ledger
{
    /// <inheritdoc />
    public partial class AccountKindCatalogHybridFoundation : Migration
    {
        private static readonly Guid CheckingKindId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid SavingsKindId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid CreditCardKindId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid CashKindId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        private static readonly Guid InvestmentKindId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private static readonly Guid ExpenseCategoryKindId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        private static readonly Guid IncomeSourceKindId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        private static readonly Guid MortgageKindId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        private static readonly Guid LoanKindId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        private static readonly Guid OtherKindId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly string OtherKindIdText = OtherKindId.ToString().ToUpperInvariant();

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountKinds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    LegacyKind = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountKinds", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AccountKinds",
                columns: new[] { "Id", "Key", "Name", "IsSystem", "IsActive", "SortOrder", "LegacyKind" },
                values: new object[,]
                {
                    { CheckingKindId, "checking", "Checking", true, true, 10, 1 },
                    { SavingsKindId, "savings", "Savings", true, true, 20, 2 },
                    { CreditCardKindId, "credit-card", "Credit Card", true, true, 30, 3 },
                    { CashKindId, "cash", "Cash", true, true, 40, 4 },
                    { InvestmentKindId, "investment", "Investment", true, true, 50, 5 },
                    { ExpenseCategoryKindId, "expense-category", "Expense Category", true, true, 60, 6 },
                    { IncomeSourceKindId, "income-source", "Income Source", true, true, 70, 7 },
                    { MortgageKindId, "mortgage", "Mortgage", true, true, 80, 8 },
                    { LoanKindId, "loan", "Loan", true, true, 90, 9 },
                    { OtherKindId, "other", "Other", true, true, 100, 99 }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "KindId",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: OtherKindId);

            migrationBuilder.Sql($@"
                UPDATE Accounts SET KindId = '{CheckingKindId}' WHERE Kind = 1;
                UPDATE Accounts SET KindId = '{SavingsKindId}' WHERE Kind = 2;
                UPDATE Accounts SET KindId = '{CreditCardKindId}' WHERE Kind = 3;
                UPDATE Accounts SET KindId = '{CashKindId}' WHERE Kind = 4;
                UPDATE Accounts SET KindId = '{InvestmentKindId}' WHERE Kind = 5;
                UPDATE Accounts SET KindId = '{ExpenseCategoryKindId}' WHERE Kind = 6;
                UPDATE Accounts SET KindId = '{IncomeSourceKindId}' WHERE Kind = 7;
                UPDATE Accounts SET KindId = '{MortgageKindId}' WHERE Kind = 8;
                UPDATE Accounts SET KindId = '{LoanKindId}' WHERE Kind = 9;
                UPDATE Accounts SET KindId = '{OtherKindIdText}' WHERE Kind = 99;
                UPDATE Accounts SET KindId = '{OtherKindIdText}' WHERE KindId = '00000000-0000-0000-0000-000000000000';
            ");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_KindId",
                table: "Accounts",
                column: "KindId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountKinds_IsActive_SortOrder_Name",
                table: "AccountKinds",
                columns: new[] { "IsActive", "SortOrder", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountKinds_Key",
                table: "AccountKinds",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_AccountKinds_KindId",
                table: "Accounts",
                column: "KindId",
                principalTable: "AccountKinds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 99);

            migrationBuilder.Sql("""
                UPDATE Accounts
                SET Kind = COALESCE((
                    SELECT LegacyKind
                    FROM AccountKinds
                    WHERE AccountKinds.Id = Accounts.KindId
                ), 99);
            """);

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_AccountKinds_KindId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_KindId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "KindId",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "AccountKinds");
        }
    }
}
