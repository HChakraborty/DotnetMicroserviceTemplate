using Microsoft.EntityFrameworkCore;
using ServiceName.Domain.Entities;

namespace ServiceName.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();
}
