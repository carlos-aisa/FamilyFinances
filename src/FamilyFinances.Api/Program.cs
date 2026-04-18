// src/FamilyFinances.Api/Program.cs
using Asp.Versioning;
using FamilyFinances.Api.Features.HostOps;
using FamilyFinances.Api.Features.Packaging;
using FamilyFinances.Infrastructure;
using FamilyFinances.Infrastructure.Persistence;
using Serilog;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Keep local development endpoint deterministic.
    // Using only HTTP avoids cross-scheme redirects that can invalidate auth flows in local setups.
    builder.WebHost.UseUrls("http://localhost:5184");
}

if (OperatingSystem.IsWindows())
{
    builder.Host.UseWindowsService();
}

PackagedConfiguration.Apply(builder.Configuration, builder.Environment.EnvironmentName);

// Serilog (read from configuration)
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Controllers + API versioning
builder.Services.AddControllers();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// Infrastructure services (DbContext, Identity, AuthN, AuthZ policies)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<ILanHostOperationsService, ScriptLanHostOperationsService>();

// Health checks (include DB check)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppIdentityDbContext>()
    .AddDbContextCheck<LedgerDbContext>();

// Swagger (development only UI)
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT auth support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FamilyFinances API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Initialize database (migrations + seed)
await DependencyInjection.InitializeAsync(app.Services);

// Serilog request logging (adds useful HTTP logs)
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Keep HTTP endpoint behavior stable across environments.
// The desktop/local workflow uses HTTP by default and API clients send bearer tokens directly.
// Redirecting to HTTPS can drop Authorization headers across scheme/port changes and cause false 401s.

// AuthN/AuthZ must be before MapControllers
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<FamilyFinances.Api.Middleware.DomainExceptionMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
// Make the implicit Program class accessible to tests
public partial class Program { }
