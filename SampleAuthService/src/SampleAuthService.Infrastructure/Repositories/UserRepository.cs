using Microsoft.EntityFrameworkCore;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Domain.Entities;
using SampleAuthService.Infrastructure.Persistence;

namespace SampleAuthService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;

    public UserRepository(AuthDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Pass cancellation token to allow request aborts or shutdown signals
        return _db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Users.FindAsync(id, cancellationToken).AsTask();
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
