using System.Security.Claims;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Features.HostOps;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinances.Web.Endpoints;

public static class LanHostOperationsEndpoints
{
    private const string AccessTokenCookieName = "ff_access_token";

    public static IEndpointRouteBuilder MapLanHostOperationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/ops/lan");

        group.MapGet("/status", GetStatusAsync);
        group.MapPost("/apply", ApplyAsync);
        group.MapPost("/certificate/regenerate", RegenerateCertificateAsync);

        return endpoints;
    }

    private static async Task<IResult> GetStatusAsync(
        HttpContext http,
        ILanHostOperationsService hostOps,
        CancellationToken ct)
    {
        if (!IsAuthorizedAdmin(http)) {
            return Results.Unauthorized();
        }

        var status = await hostOps.GetStatusAsync(ct);
        return Results.Ok(status);
    }

    private static async Task<IResult> ApplyAsync(
        [FromBody] LanAccessRequest request,
        HttpContext http,
        ILanHostOperationsService hostOps,
        CancellationToken ct)
    {
        if (!IsAuthorizedAdmin(http)) {
            return Results.Unauthorized();
        }

        var actor = ResolveActor(http);
        var result = await hostOps.ApplyAsync(request, actor, ct);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RegenerateCertificateAsync(
        [FromBody] LanAccessRequest request,
        HttpContext http,
        ILanHostOperationsService hostOps,
        CancellationToken ct)
    {
        if (!IsAuthorizedAdmin(http)) {
            return Results.Unauthorized();
        }

        if (!LanAccessCommandValidator.IsValidPort(request.HttpsPort))
        {
            return Results.BadRequest(new { error = $"Invalid HTTPS port {request.HttpsPort}." });
        }

        var actor = ResolveActor(http);
        var result = await hostOps.RegenerateCertificateAsync(request.HttpsPort, request.HostName, actor, ct);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static bool IsAuthorizedAdmin(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue(AccessTokenCookieName, out var token) ||
            string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var claims = JwtParser.ParseClaimsFromJwt(token);
        return claims.Any(IsAdminRoleClaim);
    }

    private static bool IsAdminRoleClaim(Claim claim)
    {
        if (!string.Equals(claim.Value, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(claim.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(claim.Type, "role", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(claim.Type, "roles", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(claim.Type, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveActor(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue(AccessTokenCookieName, out var token) ||
            string.IsNullOrWhiteSpace(token))
        {
            return "unknown";
        }

        var claims = JwtParser.ParseClaimsFromJwt(token);
        var email = claims.FirstOrDefault(c =>
            string.Equals(c.Type, ClaimTypes.Email, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase));

        var subject = claims.FirstOrDefault(c =>
            string.Equals(c.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase));

        return email?.Value ?? subject?.Value ?? "unknown";
    }
}
