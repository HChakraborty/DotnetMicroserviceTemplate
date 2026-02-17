using ServiceName.Domain.Enums;

namespace ServiceName.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Read operations are allowed for all roles
            options.AddPolicy("ReadPolicy",
                policy => policy.RequireRole(
                    UserRole.ReadUser.ToString(),
                    UserRole.WriteUser.ToString(),
                    UserRole.Admin.ToString()));

            // Write operations are restricted to roles that can modify data;
            options.AddPolicy("WritePolicy",
                policy => policy.RequireRole(
                    UserRole.WriteUser.ToString(),
                    UserRole.Admin.ToString()));

            // Actions limited to Admin only to prevent accidental or unauthorized system changes
            options.AddPolicy("AdminPolicy",
                policy => policy.RequireRole(
                    UserRole.Admin.ToString()));
        });
        return services;
    }
}
