using SampleAuthService.Domain.Enums;

namespace SampleAuthService.Api.Extensions;

public static class AuthorizationExtensions
{
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
}
