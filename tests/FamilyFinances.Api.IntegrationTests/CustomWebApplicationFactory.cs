using System.Collections.Generic;
using System.IO;
using System.Text;
using FamilyFinances.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FamilyFinances.Api.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;
    private bool _initialized;
    public const string TestJwtKey = "THIS_IS_A_TEST_KEY_32_CHARS_MINIMUM_123456";

    public CustomWebApplicationFactory(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_dbPath}",
                ["Jwt:Issuer"] = "FamilyFinances",
                ["Jwt:Audience"] = "FamilyFinances",
                ["Jwt:Key"] = TestJwtKey,
                // Minimal Serilog config for tests
                ["Serilog:MinimumLevel:Default"] = "Warning"
            };

            // Add test settings with high priority
            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            // Reconfigure JWT Bearer options to ensure test key is used
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "FamilyFinances",
                    ValidAudience = "FamilyFinances",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey))
                };
            });
        });
    }

    public new HttpClient CreateClient(WebApplicationFactoryClientOptions options)
    {
        // Initialize DB on first client creation
        if (!_initialized)
        {
            using var scope = Services.CreateScope();
            DependencyInjection.InitializeAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            _initialized = true;
        }

        // Disable automatic redirects to avoid HTTPS redirection issues
        options.AllowAutoRedirect = false;

        return base.CreateClient(options);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        try
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }
        catch
        {
            // Best-effort cleanup
        }
    }
}
