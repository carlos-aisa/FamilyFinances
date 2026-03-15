namespace FamilyFinances.Web.Features.HostOps;

public static class LanAccessCommandValidator
{
    public const int MinHttpsPort = 1025;
    public const int MaxHttpsPort = 65535;
    public const int ForbiddenApiPort = 5084;

    public static bool IsValidPort(int port) => port >= MinHttpsPort && port <= MaxHttpsPort && port != ForbiddenApiPort;

    public static string NormalizeHostName(string? hostName)
    {
        return string.IsNullOrWhiteSpace(hostName)
            ? Environment.MachineName
            : hostName.Trim();
    }
}
