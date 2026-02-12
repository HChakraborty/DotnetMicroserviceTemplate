using SampleAuthService.Application.DTO;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;

    public AuthService(
        IUserRepository users,
        IJwtService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    // For simplicity, we won't implement refresh tokens or token revocation. In a real app, you'd want to handle those for better security.
    public async Task<string?> GenerateTokenAsync(TokenRequestDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new ArgumentException("Invalid credentials.");

        return _jwt.GenerateToken(
            user.Id,
            user.Email,
            user.Role);
    }

    // In a real app, you'd want to validate the password strength, check for existing email, etc.
    public async Task RegisterAsync(RegisterDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user != null)
            throw new ArgumentException("Email already exists.");

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        user = new User(dto.Email, hash, "User");

        await _users.AddAsync(user);
    }

    // For simplicity, we won't implement email sending. In a real app, you'd generate a token or OTP, save it, and email it to the user.
    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.ChangePassword(
            BCrypt.Net.BCrypt.HashPassword(dto.NewPassword));

        await _users.UpdateAsync(user);

        return true;
    }
}
