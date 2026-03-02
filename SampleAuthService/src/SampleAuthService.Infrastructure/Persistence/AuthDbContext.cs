using SampleAuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SampleAuthService.Infrastructure.Persistence
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
    }
}
