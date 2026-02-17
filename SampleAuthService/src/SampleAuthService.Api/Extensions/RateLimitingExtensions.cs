using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace SampleAuthService.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimit(this IServiceCollection services) 
    { 
        services.AddRateLimiter(options => 
        { 
            options.AddFixedWindowLimiter("global", limiterOptions => 
            {
                // Limits are for template usage and can be adjust based on traffic patterns,
                // SLA requirements, and abuse protection needs of the business domain
                limiterOptions.PermitLimit = 100; 
                limiterOptions.Window = TimeSpan.FromMinutes(1); 
                limiterOptions.QueueLimit = 0; 
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; 
            }); 
        });
        return services; 
    }
}
