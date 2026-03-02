using ServiceName.Infrastructure.BackgroundServices;

namespace ServiceName.Api.Extensions.Services;

public static class MessagingEventExtensions
{
    public static IServiceCollection AddMessagingEvent(this IServiceCollection services)
    {
        services.AddHostedService<SampleCreatedConsumer>();
        services.AddHostedService<SampleDeletedConsumer>();
        services.AddHostedService<SampleUpdatedConsumer>();

        return services;
    }
}
