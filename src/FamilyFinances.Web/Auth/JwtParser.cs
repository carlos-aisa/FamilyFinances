using System.Security.Claims;
using System.Text.Json;

namespace FamilyFinances.Web.Auth;

public static class JwtParser
{
    public static IReadOnlyList<Claim> ParseClaimsFromJwt(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return Array.Empty<Claim>();

        var parts = jwt.Split('.');
        if (parts.Length != 3)
            return Array.Empty<Claim>();

        var payload = parts[1];
        var jsonBytes = DecodeBase64Url(payload);

        using var doc = JsonDocument.Parse(jsonBytes);
        var root = doc.RootElement;

        var claims = new List<Claim>();

        foreach (var property in root.EnumerateObject())
        {
            // Roles can come as array or string depending on issuer
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                {
                    claims.Add(new Claim(property.Name, item.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(property.Name, property.Value.ToString()));
            }
        }

        return claims;
    }

    private static byte[] DecodeBase64Url(string base64Url)
    {
        var padded = base64Url.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Convert.FromBase64String(padded);
    }
}
