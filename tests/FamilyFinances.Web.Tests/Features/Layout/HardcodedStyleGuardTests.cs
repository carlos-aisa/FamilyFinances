using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Layout;

public sealed class HardcodedStyleGuardTests
{
    private static readonly Regex InlineStyleRegex = new("style=\"([^\"]+)\"", RegexOptions.Compiled);
    private static readonly Regex HexRegex = new("#[0-9a-fA-F]{3,8}\\b", RegexOptions.Compiled);

    private static readonly AllowlistEntry[] InlineStyleAllowlist =
    [
        new(
            RelativePath: "src/FamilyFinances.Web/Components/Pages/Reports/AccountGroupTotalsPage.razor",
            Contains: "--ff-progress-width:@(percentage)%"),
        new(
            RelativePath: "src/FamilyFinances.Web/Components/Pages/Reports/Charts/AnnualCompositionChart.razor",
            Contains: "--ff-slice-color:{slice.ColorHex}"),
        new(
            RelativePath: "src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor",
            Contains: "--ff-progress-width:@GetPercentageWidth(row.AmountCents, maximum)")
    ];

    private static readonly AllowlistEntry[] HexAllowlist =
    [
        new(
            RelativePath: "src/FamilyFinances.Web/wwwroot/js/reportCharts.js",
            Contains: "fallback: \"#adb5bd\""),
        new(
            RelativePath: "src/FamilyFinances.Web/wwwroot/js/reportCharts.js",
            Contains: "fallback: \"#223149\""),
        new(
            RelativePath: "src/FamilyFinances.Web/wwwroot/js/reportCharts.js",
            Contains: "fallback: \"#e8efff\""),
        new(
            RelativePath: "src/FamilyFinances.Web/wwwroot/js/reportCharts.js",
            Contains: "fallback: \"#1f252d\""),
        new(
            RelativePath: "src/FamilyFinances.Web/Components/Pages/Reports/AccountStateEvolutionPanel.razor",
            Contains: "data-bs-parent=\"#accountsNatureAccordion\"")
    ];

    [Fact]
    public void InlineStyles_In_Protected_Razor_Files_Must_Be_Dynamic_And_Allowlisted()
    {
        var root = GetRepositoryRoot();
        var files = Directory
            .EnumerateFiles(Path.Combine(root, "src", "FamilyFinances.Web", "Components"), "*.razor", SearchOption.AllDirectories)
            .ToList();

        var violations = new List<string>();

        foreach (var file in files)
        {
            var relativePath = GetRelativePath(root, file);
            var lines = File.ReadAllLines(file);

            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                var matches = InlineStyleRegex.Matches(line);
                foreach (Match match in matches)
                {
                    if (IsAllowlisted(InlineStyleAllowlist, relativePath, line))
                        continue;

                    violations.Add($"{relativePath}:{index + 1} -> {match.Value}");
                }
            }
        }

        violations.Should().BeEmpty("all remaining inline styles must be explicit dynamic allowlist entries");
    }

    [Fact]
    public void Protected_Frontend_Files_Must_Not_Introduce_New_Hex_Literals()
    {
        var root = GetRepositoryRoot();
        var files = GetProtectedHexFiles(root);
        var violations = new List<string>();

        foreach (var file in files)
        {
            var relativePath = GetRelativePath(root, file);
            var lines = File.ReadAllLines(file);

            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                var matches = HexRegex.Matches(line);
                foreach (Match match in matches)
                {
                    if (IsAllowlisted(HexAllowlist, relativePath, line))
                        continue;

                    violations.Add($"{relativePath}:{index + 1} -> {match.Value}");
                }
            }
        }

        violations.Should().BeEmpty("hex literals in protected files are forbidden unless explicitly allowlisted");
    }

    [Fact]
    public void Style_Allowlists_Must_Be_PathScoped_And_Reference_Existing_Files()
    {
        var root = GetRepositoryRoot();
        var allEntries = InlineStyleAllowlist.Concat(HexAllowlist).ToList();

        allEntries.Should().OnlyContain(entry => !entry.RelativePath.Contains('*') && !entry.RelativePath.Contains('?'));
        allEntries.Should().OnlyContain(entry => entry.RelativePath.StartsWith("src/FamilyFinances.Web/", StringComparison.Ordinal));

        foreach (var entry in allEntries)
        {
            var absolute = Path.Combine(root, entry.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            File.Exists(absolute).Should().BeTrue($"allowlist path {entry.RelativePath} must exist");
            entry.Contains.Should().NotBeNullOrWhiteSpace();
        }
    }

    private static bool IsAllowlisted(IEnumerable<AllowlistEntry> allowlist, string relativePath, string line)
    {
        return allowlist.Any(entry =>
            string.Equals(entry.RelativePath, relativePath, StringComparison.Ordinal)
            && line.Contains(entry.Contains, StringComparison.Ordinal));
    }

    private static List<string> GetProtectedHexFiles(string root)
    {
        var files = new List<string>();

        var components = Path.Combine(root, "src", "FamilyFinances.Web", "Components");
        files.AddRange(Directory.EnumerateFiles(components, "*.razor", SearchOption.AllDirectories));
        files.AddRange(Directory.EnumerateFiles(components, "*.razor.css", SearchOption.AllDirectories));

        files.Add(Path.Combine(root, "src", "FamilyFinances.Web", "wwwroot", "css", "app.css"));
        files.Add(Path.Combine(root, "src", "FamilyFinances.Web", "wwwroot", "css", "premium-theme.css"));
        files.Add(Path.Combine(root, "src", "FamilyFinances.Web", "wwwroot", "js", "reportCharts.js"));

        return files.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "FamilyFinances.sln")))
        {
            current = current.Parent;
        }

        current.Should().NotBeNull("tests should execute from within repository tree");
        return current!.FullName;
    }

    private static string GetRelativePath(string root, string file)
    {
        var relative = Path.GetRelativePath(root, file);
        return relative.Replace('\\', '/');
    }

    private sealed record AllowlistEntry(string RelativePath, string Contains);
}
