using SampleAuthService.Application.DTO;
using SampleAuthService.Application.Interfaces;

namespace SampleAuthService.Application.Services;

public class TokenService : ITokenService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;

    public TokenService(
        IUserRepository users,
        IJwtService jwt)
    {
        _users = users;
        _jwt = jwt;
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

        return new TokenResponseDto()
        {
           AccessToken = token
        };
    }
}
