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

    public Task<User?> GetByEmailAsync(string email)
    {
        return _db.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return _db.Users.FindAsync(id).AsTask();
    }

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }
}
