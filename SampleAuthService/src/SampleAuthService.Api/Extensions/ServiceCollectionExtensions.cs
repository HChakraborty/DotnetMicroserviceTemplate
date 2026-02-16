using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Application.Services;
using SampleAuthService.Domain.Enums;
using SampleAuthService.Infrastructure.Persistence;
using SampleAuthService.Infrastructure.Repositories;
using SampleAuthService.Infrastructure.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace SampleAuthService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Application Layer
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    // Infrastructure Layer
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString =
            config.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' is missing.");
        }

        // DbContext
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // JWT Options
        services.Configure<JwtOptions>(
            config.GetSection("Jwt"));

        return services;
    }

    public static IServiceCollection AddControllersOptions(this IServiceCollection services)
    {
        services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters
                .Add(new JsonStringEnumConverter());
        });
        return services;
    }

    public static IServiceCollection AddRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("global", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });
        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSection = config.GetSection("Jwt");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Key"]!))
                };
            });
        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ReadPolicy",
                policy => policy.RequireRole(
                    UserRole.ReadUser.ToString(),
                    UserRole.WriteUser.ToString(),
                    UserRole.Admin.ToString()));

            options.AddPolicy("WritePolicy",
                policy => policy.RequireRole(
                    UserRole.WriteUser.ToString(),
                    UserRole.Admin.ToString()));

            options.AddPolicy("AdminPolicy",
                policy => policy.RequireRole(
                    UserRole.Admin.ToString()));
        });
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SampleAuthService API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Scheme = "bearer",
                Description = "Enter: Bearer {your JWT token}",
                BearerFormat = "JWT"
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
        return services;
    }
}
