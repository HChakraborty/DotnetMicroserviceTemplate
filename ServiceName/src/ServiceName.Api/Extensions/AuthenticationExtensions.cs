using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ServiceName.Api.Extensions;

public static class AuthenticationExtensions
{
    // Here we are doing JWT Bearer authentication but you can
    // use authentication other schemes (e.g., cookies, OAuth, API keys, mTLS) depending on business requirements and security architecture
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSection = config.GetSection("Jwt");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Key"]!))
                };
            });
        return services;
    }
}
