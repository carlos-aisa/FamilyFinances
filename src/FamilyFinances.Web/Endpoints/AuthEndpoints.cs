using Microsoft.AspNetCore.Mvc;

namespace FamilyFinances.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/auth");

        group.MapPost("/session", LoginAsync);
        group.MapGet("/session", GetSessionAsync);
        group.MapDelete("/session", LogoutAsync);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        HttpContext http,
        IHttpClientFactory factory,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request?.Password))
            return Results.BadRequest(new { error = "Email and password are required" });

        var api = factory.CreateClient("FamilyFinancesApi");
        var response = await api.PostAsJsonAsync("api/v1/auth/login", request, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return Results.Problem(
                statusCode: (int)response.StatusCode,
                detail: $"Authentication failed: {errorContent}");
        }

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            return Results.Problem("Missing access token in response");

        // Store token in HttpOnly cookie
        http.Response.Cookies.Append("ff_access_token", payload.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = http.Request.IsHttps, // Use secure cookies in production
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(8),
            Path = "/"
        });

        return Results.Ok(new { accessToken = payload.AccessToken });
    }

    private static IResult GetSessionAsync(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue("ff_access_token", out var token) ||
            string.IsNullOrWhiteSpace(token))
        {
            return Results.NoContent();
        }

        return Results.Ok(new { accessToken = token });
    }

    private static IResult LogoutAsync(HttpContext http)
    {
        http.Response.Cookies.Delete("ff_access_token");
        return Results.Ok();
    }
}

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string AccessToken);
