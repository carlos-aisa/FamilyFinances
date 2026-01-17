using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinances.Infrastructure.Persistence.Migrations.Ledger
{
    /// <inheritdoc />
    public partial class AccountNormalizedName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Name",
                table: "Accounts");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Accounts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_NormalizedName",
                table: "Accounts",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_NormalizedName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name");
        }
    }
}
