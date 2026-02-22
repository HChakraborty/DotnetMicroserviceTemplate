using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Infrastructure.Caching;
using SampleAuthService.Infrastructure.Configuration;
using SampleAuthService.Infrastructure.Persistence;
using SampleAuthService.Infrastructure.Repositories;
using SampleAuthService.Infrastructure.Security;
using StackExchange.Redis;

namespace SampleAuthService.Api.Extensions.Services;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
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

        services.AddScoped<IUserRepository, UserRepository>();

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

        services.Configure<JwtOptions>(
            config.GetSection("Jwt"));

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
