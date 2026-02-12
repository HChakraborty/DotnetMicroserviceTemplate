using SampleAuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SampleAuthService.Infrastructure.Persistence
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
    }
}
