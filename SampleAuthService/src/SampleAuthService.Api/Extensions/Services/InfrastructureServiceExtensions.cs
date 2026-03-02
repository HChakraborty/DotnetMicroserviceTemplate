using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Infrastructure.Caching;
using SampleAuthService.Infrastructure.Configuration;
using SampleAuthService.Infrastructure.Messaging;
using SampleAuthService.Infrastructure.Persistence;
using SampleAuthService.Infrastructure.Repositories;
using SampleAuthService.Infrastructure.Security;
using StackExchange.Redis;

namespace SampleAuthService.Api.Extensions.Services;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddSqlInfrastructure(config);
        services.AddRabbitMqInfrastructure(config);
        services.AddRedisInfrastructure(config);

        services.AddScoped<IUserRepository, UserRepository>();

        services.Configure<JwtOptions>(
            config.GetSection("Jwt"));

        return services;
    }

    public static IServiceCollection AddSqlInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString =
            config.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' is missing.");
        }

        // Let EF auto-detect migrations from AppDbContext assembly
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

        return services;
    }

    public static IServiceCollection AddRabbitMqInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RabbitMqSettings>(
            config.GetSection("RabbitMq"));

        services.AddSingleton<IEventBus>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

            const int maxRetries = 15;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return RabbitMqEventBus.CreateAsync(settings)
                        .GetAwaiter()
                        .GetResult();
                }
                catch
                {
                    Thread.Sleep(3000); // wait 3 sec
                }
            }

            throw new Exception("RabbitMQ not reachable after retries.");
        });

        return services;
    }

    // Redis is used only as a cache (performance optimization).
    // The service must remain functional even if Redis is unavailable.
    // Therefore we DO NOT block startup waiting for Redis.
    // 
    // AbortOnConnectFail = false enables lazy connection so the app can start
    // and cache operations will gracefully fallback to DB if Redis is down.
    //
    // In contrast:
    // - SQL Server is required for core data → startup retry enabled
    // - RabbitMQ is important for event delivery(If other services internally or externally depend upon it) → startup retry enabled

    public static IServiceCollection AddRedisInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var redisConnection =
            config.GetSection("Redis")["ConnectionString"];

        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            throw new InvalidOperationException(
                "Redis connection string is missing.");
        }

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}
