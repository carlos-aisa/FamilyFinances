using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinances.Infrastructure.Persistence.Migrations.Ledger
{
    /// <inheritdoc />
    public partial class AccountNameUniqueOnlyForActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop previous unique index if it exists
            migrationBuilder.Sql("""
            DROP INDEX IF EXISTS IX_Accounts_NormalizedName;
            """);

            migrationBuilder.Sql("""
            DROP INDEX IF EXISTS IX_Accounts_NormalizedName_Active;
            """);

            // Create filtered unique index for active accounts only
            migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS IX_Accounts_NormalizedName_Active
            ON Accounts (NormalizedName)
            WHERE IsClosed = 0;
            """);
                    
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            DROP INDEX IF EXISTS IX_Accounts_NormalizedName_Active;
            """);

            // Optionally recreate the original unique index (if that was the previous state)
            migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS IX_Accounts_NormalizedName
            ON Accounts (NormalizedName);
            """);
        }
    }
}
