using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FamilyFinances.Infrastructure.Persistence.Repositories;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components;
using FamilyFinances.Web.Endpoints;
using FamilyFinances.Web.Features.Localization;
using FamilyFinances.Web.State;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddLocalization();

builder.Services.Configure<ApiClientOptions>(builder.Configuration.GetSection("Api"));

builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IApiTokenStore, ApiTokenStore>();
builder.Services.AddScoped<ISessionInitializationService, SessionInitializationService>();

// API clients
builder.Services.AddScoped<IAccountsApi, AccountsApi>();
builder.Services.AddScoped<PayeesApi>();
builder.Services.AddScoped<TransactionsApi>();
builder.Services.AddScoped<ReportsApi>();
builder.Services.AddScoped<BackupApi>();
builder.Services.AddScoped<AccountGroupsApi>();
builder.Services.AddScoped<HistoryApi>();
builder.Services.AddSingleton<HistoryRefreshNotifier>();

// Authentication
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddHttpClient("FamilyFinancesApi", (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ApiClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Note: do NOT register application/infrastructure handlers or repositories that depend on DbContext here.

var app = builder.Build();

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(WebLocalizationOptions.DefaultCulture),
    SupportedCultures = WebLocalizationOptions.SupportedCultures.ToList(),
    SupportedUICultures = WebLocalizationOptions.SupportedCultures.ToList(),
    RequestCultureProviders =
    [
        new CookieRequestCultureProvider
        {
            CookieName = CookieRequestCultureProvider.DefaultCookieName
        },
        new AcceptLanguageHeaderRequestCultureProvider()
    ]
};

app.UseRequestLocalization(localizationOptions);

// Only use HTTPS redirection in Development (when we have proper HTTPS setup)
// Skip in Production (ZIP distribution uses HTTP-only for local access)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAuthEndpoints();

app.Run();

public sealed class ApiClientOptions
{
    public string BaseUrl { get; set; } = "";
}
