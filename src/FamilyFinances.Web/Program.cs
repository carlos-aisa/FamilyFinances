using System.Net.Http.Headers;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorizationCore();

builder.Services.Configure<ApiClientOptions>(builder.Configuration.GetSection("Api"));

builder.Services.AddScoped<IApiTokenStore, ApiTokenStore>();

builder.Services.AddScoped<AuthApi>();
builder.Services.AddScoped<AccountsApi>();
builder.Services.AddScoped<JwtAuthStateProvider>();

// Also register it as the framework abstraction.
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddHttpClient("FamilyFinancesApi", (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ApiClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler(sp =>
{
    var tokenStore = sp.GetRequiredService<IApiTokenStore>();
    return new AuthHeaderHandler(tokenStore);
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public sealed class ApiClientOptions
{
    public string BaseUrl { get; set; } = "";
}
