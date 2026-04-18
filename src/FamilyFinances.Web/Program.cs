using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FamilyFinances.Infrastructure.Persistence.Repositories;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components;
using FamilyFinances.Web.Endpoints;
using FamilyFinances.Web.Features.Localization;
using FamilyFinances.Web.Features.HostOps;
using FamilyFinances.Web.Features.Packaging;
using FamilyFinances.Web.State;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

const string DefaultApiBaseUrl = "http://127.0.0.1:5084/";
const string DevelopmentApiHttpBaseUrl = "http://localhost:5184/";
const string DevelopmentApiHttpsBaseUrl = "https://localhost:7349/";
const int DevelopmentApiHttpPort = 5184;
const int DevelopmentApiHttpsPort = 7349;

PackagedConfiguration.Apply(builder.Configuration, builder.Environment.EnvironmentName);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddLocalization();

builder.Services.Configure<ApiClientOptions>(builder.Configuration.GetSection("Api"));
builder.Services.PostConfigure<ApiClientOptions>(options =>
{
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        options.BaseUrl = builder.Environment.IsDevelopment()
            ? DevelopmentApiHttpsBaseUrl
            : DefaultApiBaseUrl;
    }

    if (!builder.Environment.IsDevelopment())
        return;

    if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var configuredUri))
        return;

    var isLocalHost = string.Equals(configuredUri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(configuredUri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
    if (!isLocalHost)
        return;

    var httpsAvailable = ApiClientEndpointProbe.CanReachTcpPort("127.0.0.1", DevelopmentApiHttpsPort);
    if (string.Equals(configuredUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
        configuredUri.Port == DevelopmentApiHttpPort &&
        httpsAvailable)
    {
        options.BaseUrl = DevelopmentApiHttpsBaseUrl;
        return;
    }

    if (string.Equals(configuredUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
        configuredUri.Port == DevelopmentApiHttpsPort &&
        !httpsAvailable)
    {
        options.BaseUrl = DevelopmentApiHttpBaseUrl;
    }
});

builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IApiTokenStore, ApiTokenStore>();
builder.Services.AddScoped<ISessionInitializationService, SessionInitializationService>();
builder.Services.AddScoped<ILanHostOperationsService, ApiLanHostOperationsService>();

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
    var baseUrl = options.BaseUrl?.Trim();
    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
    {
        throw new InvalidOperationException(
            $"Invalid Api:BaseUrl value '{baseUrl ?? "<null>"}'. " +
            $"Expected an absolute URI such as '{DefaultApiBaseUrl}'.");
    }

    client.BaseAddress = baseUri;
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Keep redirects explicit so config mistakes are visible instead of turning into 401 loops.
    AllowAutoRedirect = false
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
        }
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
app.MapLanHostOperationsEndpoints();

app.Run();

// Make the implicit Program class accessible to tests.
public partial class Program { }

public sealed class ApiClientOptions
{
    public string BaseUrl { get; set; } = "";
}

internal static class ApiClientEndpointProbe
{
    public static bool CanReachTcpPort(string host, int port)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var completed = connectTask.Wait(TimeSpan.FromMilliseconds(250));
            return completed && client.Connected;
        }
        catch
        {
            return false;
        }
    }
}
