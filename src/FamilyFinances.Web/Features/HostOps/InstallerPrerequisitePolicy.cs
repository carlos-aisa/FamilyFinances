namespace FamilyFinances.Web.Features.HostOps;

public static class InstallerPrerequisitePolicy
{
    public static bool IsRebootPending(
        bool componentBasedServicingPending,
        bool windowsUpdatePending,
        bool pendingFileRenameOperations)
    {
        return componentBasedServicingPending ||
               windowsUpdatePending ||
               pendingFileRenameOperations;
    }

    public static bool IsRestartNeeded(string? restartNeededValue)
    {
        if (string.IsNullOrWhiteSpace(restartNeededValue))
        {
            return false;
        }

        return !restartNeededValue.Equals("No", StringComparison.OrdinalIgnoreCase) &&
               !restartNeededValue.Equals("False", StringComparison.OrdinalIgnoreCase) &&
               !restartNeededValue.Equals("0", StringComparison.OrdinalIgnoreCase);
    }

    public static bool ShouldAttemptHostingBundleMaintenance(
        bool aspNetCoreModuleInstalled,
        bool rebootPending)
    {
        return !aspNetCoreModuleInstalled && !rebootPending;
    }

    public static string BuildRebootRequiredMessage(IEnumerable<string> sources)
    {
        var materialized = sources
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .ToArray();

        var suffix = materialized.Length == 0
            ? string.Empty
            : $" Detected state: {string.Join(", ", materialized)}.";

        return "Precheck failed. Windows restart is required before FamilyFinances can finish enabling IIS prerequisites. Reboot the machine and rerun setup." + suffix;
    }
}
