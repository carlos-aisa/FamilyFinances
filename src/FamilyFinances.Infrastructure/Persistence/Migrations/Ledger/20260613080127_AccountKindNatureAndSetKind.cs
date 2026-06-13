using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinances.Infrastructure.Persistence.Migrations.Ledger
{
    /// <inheritdoc />
    public partial class AccountKindNatureAndSetKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Nature",
                table: "AccountKinds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.Sql("""
                UPDATE AccountKinds SET Nature = 1 WHERE Key IN ('checking', 'savings', 'cash', 'investment');
                UPDATE AccountKinds SET Nature = 2 WHERE Key IN ('credit-card', 'mortgage', 'loan');
                UPDATE AccountKinds SET Nature = 3 WHERE Key = 'income-source';
                UPDATE AccountKinds SET Nature = 4 WHERE Key = 'expense-category';
                UPDATE AccountKinds SET Nature = 5 WHERE Key = 'other';

                UPDATE AccountKinds
                SET Nature = COALESCE((
                    SELECT a.Nature
                    FROM Accounts a
                    WHERE a.KindId = AccountKinds.Id
                    LIMIT 1
                ), 4)
                WHERE IsSystem = 0;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nature",
                table: "AccountKinds");
        }
    }
}
