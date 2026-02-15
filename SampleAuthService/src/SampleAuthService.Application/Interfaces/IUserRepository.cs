using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(Guid id);

    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
