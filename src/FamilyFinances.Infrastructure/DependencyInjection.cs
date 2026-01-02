
using System.Text;
using FamilyFinances.Application.Abstractions;
using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Create;
using FamilyFinances.Application.Ledger.Accounts.List;
using FamilyFinances.Application.Ledger.Transactions.Create;
using FamilyFinances.Infrastructure.Persistence;
using FamilyFinances.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddIdentityServices();
        services.AddJwtAuthentication(configuration);
        services.AddAuthorizationPolicies();
        services.AddScoped<ILedgerUnitOfWork, LedgerUnitOfWork>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        services.AddScoped<CreateAccountHandler>();
        services.AddScoped<ListAccountsHandler>();
        services.AddScoped<CreateTransactionHandler>();
        services.AddScoped<GetTransactionByIdHandler>();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));
        
        services.AddDbContext<LedgerDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Developer-friendly defaults for v0.1.0 (tighten later if needed)
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"]!;
        var audience = jwtSection["Audience"]!;
        var key = jwtSection["Key"]!;

        if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters long.");
        }

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

        return services;
    }

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.CanRead, p => p.RequireRole(Roles.Admin, Roles.Reader));
            options.AddPolicy(Policies.CanWrite, p => p.RequireRole(Roles.Admin));
        });

        return services;
    }

    // Optional: keep initialization in Infrastructure (migrations + seeding).
    // If you already have this method elsewhere, remove this and use your existing initializer.
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        await dbContext.Database.MigrateAsync();

        var ledgerDb = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
        await ledgerDb.Database.MigrateAsync();

        // Seed roles + default admin user
        await Infrastructure.Identity.IdentitySeeder.SeedAsync(scope.ServiceProvider);
    }
}
