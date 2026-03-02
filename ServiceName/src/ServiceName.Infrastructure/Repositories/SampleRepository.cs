using Microsoft.EntityFrameworkCore;
using ServiceName.Domain.Entities;
using ServiceName.Infrastructure.Persistence;
using ServiceName.Application.Interfaces;

namespace ServiceName.Infrastructure.Repositories;

public class SampleRepository(AppDbContext dbContext) : IRepository<SampleEntity>
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<SampleEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Pass cancellation token to allow request aborts or shutdown signals
        return await _dbContext.SampleEntities.ToListAsync(cancellationToken);
    }

    public async Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SampleEntities.FirstOrDefaultAsync(x => x.Id == id , cancellationToken);
    }

    public async Task AddAsync(SampleEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.SampleEntities.AddAsync(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SampleEntity entity, CancellationToken cancellationToken = default)
    {
        _dbContext.SampleEntities.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.SampleEntities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity != null)
        {
            _dbContext.SampleEntities.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
