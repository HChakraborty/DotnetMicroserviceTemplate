using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(User user);
}
