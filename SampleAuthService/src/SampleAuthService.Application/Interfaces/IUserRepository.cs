using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(User user, CancellationToken cancellationToken = default);
}
