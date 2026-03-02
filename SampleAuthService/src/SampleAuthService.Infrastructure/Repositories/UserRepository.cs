using Microsoft.EntityFrameworkCore;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Domain.Entities;
using SampleAuthService.Infrastructure.Persistence;

namespace SampleAuthService.Infrastructure.Repositories;

public class UserRepository(AuthDbContext dbContext) : IUserRepository
{
    private readonly AuthDbContext _dbContext = dbContext;

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Pass cancellation token to allow request aborts or shutdown signals
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FindAsync(id, cancellationToken).AsTask();
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
