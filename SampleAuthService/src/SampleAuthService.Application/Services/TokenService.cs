using Microsoft.Extensions.Configuration;
using SampleAuthService.Application.DTO.TokenDto;
using SampleAuthService.Application.Interfaces;

namespace SampleAuthService.Application.Services;

public class TokenService : ITokenService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public TokenService(
        IUserRepository users,
        IJwtService jwt,
        IConfiguration config)
    {
        _users = users;
        _jwt = jwt;
        _config = config;
    }

    // For simplicity, we won't implement refresh tokens or token revocation. In a real app, you'd want to handle those for better security.
    public async Task<TokenResponseDto?> GenerateTokenAsync(TokenRequestDto dto)
    {
        var user = await _users.GetUserByEmailAsync(dto.Email);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new ArgumentException("Invalid credentials.");

        var token = _jwt.GenerateToken(user);

        // Token expires in 30 minutes or 1800 seconds
        // Can be configured in appsettings.json
        var tokenRespose = new TokenResponseDto()
        {
            AccessToken = token,
            ExpiresAt = _config.GetValue<int>("Jwt:ExpireMinutes") * 60
        };

        return tokenRespose;
    }
}
