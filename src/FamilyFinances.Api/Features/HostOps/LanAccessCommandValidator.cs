namespace FamilyFinances.Api.Features.HostOps;

public static class LanAccessCommandValidator
{
    public const int MinHttpsPort = 1025;
    public const int MaxHttpsPort = 65535;
    public const int ForbiddenApiPort = 5084;

    public static bool IsValidPort(int port) =>
        port >= MinHttpsPort &&
        port <= MaxHttpsPort &&
        port != ForbiddenApiPort;

    public static string NormalizeHostName(string? hostName)
    {
        var normalized = string.IsNullOrWhiteSpace(hostName)
            ? Environment.MachineName
            : hostName.Trim();

        if (!IsSafeHostName(normalized))
        {
            throw new ArgumentException("Invalid host name. Use a valid DNS name or IP address.", nameof(hostName));
        }

        return normalized;
    }

    public static bool IsSafeHostName(string? hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            return false;
        }

        var candidate = hostName.Trim();
        if (candidate.Length > 253)
        {
            return false;
        }

        if (candidate.StartsWith('-') || candidate.StartsWith('.') ||
            candidate.EndsWith('-') || candidate.EndsWith('.'))
        {
            return false;
        }

        if (candidate.IndexOfAny(['"', '\'', '`', ';', '|', '&', '$']) >= 0)
        {
            return false;
        }

        var hostType = Uri.CheckHostName(candidate);
        return hostType is UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6;
    }
}
