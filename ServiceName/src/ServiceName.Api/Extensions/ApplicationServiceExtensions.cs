using ServiceName.Application.Interfaces;
using ServiceName.Application.Services;

namespace ServiceName.Api.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISampleService, SampleService>();

        return services;
    }
}
