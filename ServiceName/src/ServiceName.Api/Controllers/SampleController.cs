using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceName.Application.DTO;
using ServiceName.Application.Events;
using ServiceName.Application.Interfaces;

namespace ServiceName.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/samples")]
public class SampleController: ControllerBase
{
    private readonly ISampleService _service;
    private readonly IEventBus _eventBus;

    public SampleController(ISampleService service, IEventBus eventBus)
    {
        _service = service;
        _eventBus = eventBus;
    }

    [Authorize(Policy = "ReadPolicy")]
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [Authorize(Policy = "ReadPolicy")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null) 
            return NotFound();

        return Ok(result);
    }


    // We have added DTOs for Add, Update and Delete.
    // They look same for now but in real world they can differ and it's a good practice to separate them.
    [Authorize(Policy = "WritePolicy")]
    [HttpPost]
    public async Task<IActionResult> AddAsync(AddSampleRequestDto dto)
    {
        var id = await _service.AddAsync(dto);

        await _eventBus.PublishAsync(
            new SampleCreatedEvent(id));

        var response = new AddSampleResponseDto 
        {   
            Message = "Sample created successfully.",
            Id = id 
        };

        return Ok(response);
    }

    [Authorize(Policy = "WritePolicy")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateSampleRequestDto dto)
    {
        // Prevent inconsistent updates if client sends mismatched ids
        if (dto.Id != Guid.Empty && dto.Id != id)
            return BadRequest("Route id and body id must match.");

        dto.Id = id;

        await _service.UpdateAsync(dto);

        await _eventBus.PublishAsync(
            new SampleUpdatedEvent(dto.Id));

        var response = new UpdateSampleResponseDto 
        {   
            Message = "Sample updated successfully.",
        };

        return Ok(response);
    }

    [Authorize(Policy = "AdminPolicy")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteByIdAsync(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        await _service.DeleteByIdAsync(id);

        if (result != null)
        {
            await _eventBus.PublishAsync(
                new SampleDeletedEvent(result.Id));
        }

        var response = new DeleteSampleResponseDto 
        {   
            Message = "Sample deleted successfully.",
        };

        return Ok(response);
    }
}
