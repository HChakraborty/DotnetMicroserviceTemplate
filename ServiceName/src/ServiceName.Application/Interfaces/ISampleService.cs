using ServiceName.Application.DTO;

namespace ServiceName.Application.Interfaces;

public interface ISampleService
{
    Task<IReadOnlyList<GetSampleRequestDto>> GetAllAsync();
    Task<GetSampleRequestDto?> GetByIdAsync(Guid id);
    Task<Guid> AddAsync(AddSampleRequestDto dto);
    Task UpdateAsync(UpdateSampleRequestDto dto);
    Task DeleteByIdAsync(Guid id);
}
