using SampleAuthService.Application.DTO.UserDto;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;

    public UserService(
        IUserRepository users,
        IJwtService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    // In a real app, you'd want to validate the password strength, check for existing email, etc.
    public async Task RegisterUserAsync(RegisterUserDto dto)
    {
        var user = await _users.GetUserByEmailAsync(dto.Email);

        if (user != null)
            throw new ArgumentException("Email already exists.");

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        user = new User(dto.Email, hash, dto.Role);

        await _users.AddUserAsync(user);
    }

    // For simplicity, we won't implement email sending. In a real app, you'd generate a token or OTP, save it, and email it to the user.
    public async Task<bool> ResetPasswordRequestAsync(ResetPasswordDto dto)
    {
        var user = await _users.GetUserByEmailAsync(dto.Email);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.ChangePassword(
            BCrypt.Net.BCrypt.HashPassword(dto.NewPassword));

        await _users.UpdateUserAsync(user);

        return true;
    }

    public async Task<bool> DeleteUserAsync(string email)
    {
        var user = await _users.GetUserByEmailAsync(email);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        await _users.DeleteUserAsync(user);

        return true;
    }

    public async Task<GetUserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _users.GetUserByEmailAsync(email);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        return new GetUserDto()
        {
            Email = user.Email,
            Role = user.Role
        };
    }
}
