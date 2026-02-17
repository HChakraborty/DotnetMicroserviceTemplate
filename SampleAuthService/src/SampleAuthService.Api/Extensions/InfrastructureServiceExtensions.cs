using Microsoft.EntityFrameworkCore;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Infrastructure.Persistence;
using SampleAuthService.Infrastructure.Repositories;
using SampleAuthService.Infrastructure.Security;

namespace SampleAuthService.Api.Extensions;

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

        services.Configure<JwtOptions>(
            config.GetSection("Jwt"));

        return services;
    }
}
