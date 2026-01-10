using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components;
using FamilyFinances.Web.Endpoints;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ApiClientOptions>(builder.Configuration.GetSection("Api"));

builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IApiTokenStore, ApiTokenStore>();

builder.Services.AddScoped<IAccountsApi, AccountsApi>();
builder.Services.AddScoped<PayeesApi>();
builder.Services.AddScoped<TransactionsApi>();

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
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map authentication endpoints
app.MapAuthEndpoints();

app.Run();

public sealed class ApiClientOptions
{
    public string BaseUrl { get; set; } = "";
}
