// src/FamilyFinances.Api/Program.cs
using Asp.Versioning;
using FamilyFinances.Infrastructure;
using FamilyFinances.Infrastructure.Persistence;
using Serilog;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

// Only use HTTPS redirection in Development (when we have proper HTTPS setup)
// Skip in Testing and Production (ZIP distribution uses HTTP-only for local access)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// AuthN/AuthZ must be before MapControllers
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<FamilyFinances.Api.Middleware.DomainExceptionMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
// Make the implicit Program class accessible to tests
public partial class Program { }