using FamilyFinances.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Accounts;

public sealed class AccountKindMigrationTests
{
    [Fact]
    public async Task Migration_Backfills_AccountKindId_FromLegacyKindColumn()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ff-kind-migration-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbPath}";

        try
        {
            var options = new DbContextOptionsBuilder<LedgerDbContext>()
                .UseSqlite(connectionString)
                .Options;

            await using (var context = new LedgerDbContext(options))
            {
                await context.Database.MigrateAsync("20260218062408_FiscalYearGovernance");

                var accountId = Guid.NewGuid();
                await context.Database.ExecuteSqlRawAsync(
                    """
                    INSERT INTO Accounts (Id, Name, Nature, Kind, OpenedOn, IsClosed, ClosedOn, NormalizedName)
                    VALUES ({0}, {1}, {2}, {3}, {4}, {5}, NULL, {6});
                    """,
                    accountId,
                    "Legacy Checking",
                    1,
                    1,
                    "2026-01-02",
                    0,
                    "LEGACY CHECKING");

                await context.Database.MigrateAsync();

                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT KindId FROM Accounts WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", accountId);
                var kindIdRaw = await cmd.ExecuteScalarAsync();

                kindIdRaw.Should().NotBeNull();
                kindIdRaw!.ToString().Should().NotBe(Guid.Empty.ToString());

                await using var verifyCmd = connection.CreateCommand();
                verifyCmd.CommandText =
                    """
                    SELECT COUNT(*)
                    FROM Accounts a
                    JOIN AccountKinds k ON k.Id = a.KindId
                    WHERE a.Id = $id AND k.Key = 'checking' AND k.LegacyKind = 1 AND k.Nature = 1;
                    """;
                verifyCmd.Parameters.AddWithValue("$id", accountId);

                var matchCount = Convert.ToInt32(await verifyCmd.ExecuteScalarAsync());
                matchCount.Should().Be(1);
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                await TryDeleteFileAsync(dbPath);
        }
    }

    private static async Task TryDeleteFileAsync(string path)
    {
        const int attempts = 8;

        for (var i = 0; i < attempts; i++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch (IOException) when (i < attempts - 1)
            {
                await Task.Delay(50);
            }
            catch (IOException)
            {
                return;
            }
        }
    }
}
