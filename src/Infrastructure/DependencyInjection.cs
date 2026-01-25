using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SellerInventory.Application.Interfaces;
using SellerInventory.Infrastructure.Auth;
using SellerInventory.Infrastructure.Data;
using SellerInventory.Infrastructure.Repositories;
using SellerInventory.Infrastructure.Storage;
using SellerInventory.Infrastructure.Tenancy;

namespace SellerInventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration - PostgreSQL only (NEON for production)
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=sellerinventory;Username=postgres;Password=password";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Enable retry on transient failures (NEON free tier can sleep)
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);

                // Command timeout for slow wake-up
                npgsqlOptions.CommandTimeout(60);
            });
        });

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Tenant context (scoped per request)
        services.AddScoped<ITenantContext, TenantContext>();

        // Auth services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        // Storage service - supports both Local (dev) and GoogleCloud (production)
        var storageProvider = configuration.GetValue<string>("StorageProvider") ?? "Local";
        if (storageProvider.Equals("GoogleCloud", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IStorageService, GoogleCloudStorageService>();
        }
        else
        {
            services.AddSingleton<IStorageService, LocalStorageService>();
        }

        // JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";

        services.AddAuthentication(options =>
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
                ValidIssuer = jwtSettings["Issuer"] ?? "SellerInventory",
                ValidAudience = jwtSettings["Audience"] ?? "SellerInventory",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

        services.AddAuthorization();

        return services;
    }
}
