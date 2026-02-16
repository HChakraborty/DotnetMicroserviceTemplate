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

    public Task<User?> GetUserByEmailAsync(string email)
    {
        return _db.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return _db.Users.FindAsync(id).AsTask();
    }

    public async Task AddUserAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(User user)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }
}
