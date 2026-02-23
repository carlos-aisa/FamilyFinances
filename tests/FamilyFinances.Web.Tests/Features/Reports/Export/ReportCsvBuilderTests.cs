using FamilyFinances.Web.Features.Reports;
using FamilyFinances.Web.Features.Reports.Export;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports.Export;

public sealed class ReportCsvBuilderTests
{
    [Fact]
    public void Build_Includes_Context_Headers_And_Row_Values()
    {
        var displayedAmount = MoneyFormatter.FormatCentsWithSign(123_456);

        var csv = ReportCsvBuilder.Build(
            reportName: "Account Totals",
            context: new Dictionary<string, string?>
            {
                ["From Date"] = "2026-02-01",
                ["To Date (exclusive)"] = "2026-03-01"
            },
            headers: ["Account Name", "Net Change"],
            rows:
            [
                ["Main Bank", displayedAmount]
            ]);

        csv.Should().Contain("# Report: Account Totals");
        csv.Should().Contain("# From Date: 2026-02-01");
        csv.Should().Contain("Account Name,Net Change");
        csv.Should().Contain("Main Bank");
        csv.Should().Contain(displayedAmount);
    }

    [Fact]
    public void Build_When_No_Rows_Adds_Explicit_NoData_Note()
    {
        var csv = ReportCsvBuilder.Build(
            reportName: "Empty",
            context: new Dictionary<string, string?>(),
            headers: ["A", "B"],
            rows: Array.Empty<IReadOnlyList<string?>>());

        csv.Should().Contain("A,B");
        csv.Should().Contain("# No rows available for the selected filters.");
    }

    [Fact]
    public void Build_When_Rows_Exceed_MaxRows_Adds_Truncation_Warning()
    {
        var rows = Enumerable
            .Range(1, 10_001)
            .Select(i => (IReadOnlyList<string?>)
            [
                $"Row {i}",
                i.ToString()
            ])
            .ToList();

        var csv = ReportCsvBuilder.Build(
            reportName: "Large Export",
            context: new Dictionary<string, string?>(),
            headers: ["Name", "Value"],
            rows: rows);

        csv.Should().Contain("# WARNING: Output truncated to 10000 rows.");
        csv.Should().Contain("Row 10000");
        csv.Should().NotContain("Row 10001");
    }
}
