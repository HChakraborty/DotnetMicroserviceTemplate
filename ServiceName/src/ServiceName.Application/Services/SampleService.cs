using ServiceName.Application.DTO;
using ServiceName.Application.Interfaces;
using ServiceName.Domain.Entities;

namespace ServiceName.Application.Services;

public class SampleService: ISampleService
{
    private readonly IRepository<SampleEntity> _repository;
    private readonly ICacheService _cache;

    public SampleService(IRepository<SampleEntity> repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    // This method intentionally does not use caching.
    //
    // Caching large collections (GetAll) can lead to:
    // - High memory consumption
    // - Cache invalidation complexity
    // - Stale data issues in distributed systems
    //
    // In production systems, prefer:
    // - Pagination
    // - Filtering
    // - Caching individual items instead of entire lists
    public async Task<IReadOnlyList<GetSampleRequestDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return [.. entities.Select(x => new GetSampleRequestDto
        {
            Id = x.Id,
            Name = x.Name
        })];
    }

    public async Task<GetSampleRequestDto?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"sample:id:{id}";

        // Try cache first
        var cached = await _cache.GetAsync<GetSampleRequestDto>(cacheKey);
        if (cached != null)
            return cached;

        // Fallback to DB
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) 
            return null;

        var dto = new GetSampleRequestDto
        {
            Id = entity.Id,
            Name = entity.Name
        };

        // Store in cache
        await _cache.SetAsync(cacheKey, dto,
            TimeSpan.FromMinutes(10));

        return dto;
    }

    public async Task<Guid> AddAsync(AddSampleRequestDto dto)
    {
        // In real world, you should validate the input data here before saving to database.
        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name
        };

        await _repository.AddAsync(entity);

        var cacheKey = $"sample:id:{entity.Id}";

        // Invalidate item cache
        await _cache.RemoveAsync(cacheKey);

        return entity.Id;
    }

    public async Task UpdateAsync(UpdateSampleRequestDto dto)
    {
        // In real world, you should validate the input data here before saving to database.
        var entity = new SampleEntity
        {
            Id = dto.Id,
            Name = dto.Name
        };
        await _repository.UpdateAsync(entity);

        var cacheKey = $"sample:id:{dto.Id}";

        // Invalidate item cache
        await _cache.RemoveAsync(cacheKey);
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        await _repository.DeleteByIdAsync(id);

        var cacheKey = $"sample:id:{id}";

        // Invalidate item cache
        await _cache.RemoveAsync(cacheKey);
    }
}
