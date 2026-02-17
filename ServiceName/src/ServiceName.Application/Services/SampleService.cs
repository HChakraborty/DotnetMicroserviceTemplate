using ServiceName.Application.DTO;
using ServiceName.Application.Interfaces;
using ServiceName.Domain.Entities;

namespace ServiceName.Application.Services;

public class SampleService: ISampleService
{
    private readonly IRepository<SampleEntity> _repository;

    public SampleService(IRepository<SampleEntity> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<SampleDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(x => new SampleDTO
        {
            Id = x.Id,
            Name = x.name
        }).ToList();
    }

    public async Task<SampleDTO?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;
        return new SampleDTO
        {
            Id = entity.Id,
            Name = entity.name
        };
    }

    public async Task AddAsync(SampleDTO dto)
    {
        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            name = dto.Name
        };
        await _repository.AddAsync(entity);
    }

    public async Task UpdateAsync(SampleDTO dto)
    {
        var entity = new SampleEntity
        {
            Id = dto.Id,
            name = dto.Name
        };
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        await _repository.DeleteByIdAsync(id);
    }
}
