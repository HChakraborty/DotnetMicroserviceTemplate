using System.Text.Json.Serialization;

namespace SampleAuthService.Api.Extensions;

public static class ControllerExtensions
{
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
}
