using Microsoft.EntityFrameworkCore;
using ServiceName.Domain.Entities;
using ServiceName.Application.Interfaces;
using ServiceName.Infrastructure.Persistence;

namespace ServiceName.Infrastructure.Repositories;

public class SampleRepository: IRepository<SampleEntity>
{
    private readonly AppDbContext _context;

    public SampleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SampleEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Pass cancellation token to allow request aborts or shutdown signals
        return await _context.SampleEntities.ToListAsync(cancellationToken);
    }

    public async Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SampleEntities.FirstOrDefaultAsync(x => x.Id == id , cancellationToken);
    }

    public async Task AddAsync(SampleEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.SampleEntities.AddAsync(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SampleEntity entity, CancellationToken cancellationToken = default)
    {
        _context.SampleEntities.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SampleEntities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity != null)
        {
            _context.SampleEntities.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
