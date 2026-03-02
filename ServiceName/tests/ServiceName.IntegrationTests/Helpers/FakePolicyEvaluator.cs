using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ServiceName.IntegrationTests.Helpers;

// FakePolicyEvaluator overrides authorization pipeline
// because we don't use real JWT in service-level integration tests
public class FakePolicyEvaluator : IPolicyEvaluator
{
    public Task<AuthenticateResult> AuthenticateAsync(
        AuthorizationPolicy policy,
        HttpContext context)
    {
        var role = context.Request.Headers["X-Test-Role"]
            .FirstOrDefault();

        if (string.IsNullOrEmpty(role))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "integration-user"),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        AuthenticateResult authenticationResult,
        HttpContext context,
        object? resource)
    {
        if (!authenticationResult.Succeeded)
            return Task.FromResult(
                PolicyAuthorizationResult.Challenge());

        var user = authenticationResult.Principal!;

        // Evaluate role requirements
        foreach (var requirement in policy.Requirements)
        {
            if (requirement is RolesAuthorizationRequirement rolesRequirement)
            {
                var userRoles = user.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value);

                if (!userRoles.Any(r => rolesRequirement.AllowedRoles.Contains(r)))
                {
                    return Task.FromResult(
                        PolicyAuthorizationResult.Forbid());
                }
            }
        }

        return Task.FromResult(
            PolicyAuthorizationResult.Success());
    }
}