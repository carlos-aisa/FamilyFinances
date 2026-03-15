using System.Security.Cryptography;

namespace FamilyFinances.Web.Features.HostOps;

public static class InstallerSecretPolicy
{
    public const string DefaultJwtKey = "PRODUCTION_SECRET_KEY_CHANGE_THIS_IN_REAL_DEPLOYMENT_MIN_64_CHARS_0123456789ABCDEF";
    public const int MinimumJwtLength = 32;

    public static bool RequiresRotation(string? currentKey)
    {
        return string.IsNullOrWhiteSpace(currentKey) ||
               currentKey.Length < MinimumJwtLength ||
               string.Equals(currentKey, DefaultJwtKey, StringComparison.Ordinal);
    }

    public static string GenerateSecureJwtKey()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
