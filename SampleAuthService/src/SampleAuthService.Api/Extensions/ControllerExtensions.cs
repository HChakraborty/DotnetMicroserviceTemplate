using System.Text.Json.Serialization;

namespace SampleAuthService.Api.Extensions;

public static class ControllerExtensions
{
    public static IServiceCollection AddControllersOptions(this IServiceCollection services)
    {
        services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Serialize enums as strings instead of integers to make API contracts human-readable
            options.JsonSerializerOptions.Converters
                .Add(new JsonStringEnumConverter());
        });
        return services;
    }
}
