using Microsoft.EntityFrameworkCore;
using ServiceName.Application.Interfaces;
using ServiceName.Domain.Entities;
using ServiceName.Infrastructure.Persistence;
using ServiceName.Infrastructure.Repositories;

namespace ServiceName.Api.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string 'Default' is missing.");
        }

        // Let EF auto-detect migrations from AppDbContext assembly
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

        services.AddScoped<IRepository<SampleEntity>, SampleRepository>();

        return services;
    }
}
