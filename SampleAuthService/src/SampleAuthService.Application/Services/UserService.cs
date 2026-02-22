using SampleAuthService.Application.DTO.UserDto;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;
    private readonly ICacheService _cache;

    public UserService(
        IUserRepository users,
        IJwtService jwt,
        ICacheService cache)
    {
        _users = users;
        _jwt = jwt;
        _cache = cache;
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

        // 🔥 Invalidate cache (if any stale entry exists)
        var cacheKey = $"user:email:{dto.Email.ToLower()}";
        await _cache.RemoveAsync(cacheKey);
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

        // 🔥 Remove cached user
        var cacheKey = $"user:email:{email.ToLower()}";
        await _cache.RemoveAsync(cacheKey);

        return true;
    }

    public async Task<GetUserDto?> GetUserByEmailAsync(string email)
    {
        var cacheKey = $"user:email:{email.ToLower()}";

        // Try cache first
        var cached = await _cache.GetAsync<GetUserDto>(cacheKey);
        if (cached != null)
            return cached;

        // Fallback to DB
        var user = await _users.GetUserByEmailAsync(email);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var dto = new GetUserDto
        {
            Email = user.Email,
            Role = user.Role
        };

        // Store in cache
        await _cache.SetAsync(cacheKey, dto,
            TimeSpan.FromMinutes(10));

        return dto;
    }
}
