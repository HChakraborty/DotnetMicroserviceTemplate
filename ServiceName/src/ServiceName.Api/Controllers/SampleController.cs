using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceName.Application.DTO;
using ServiceName.Application.Interfaces;

namespace ServiceName.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/sample")]
public class SampleController: ControllerBase
{
    private readonly ISampleService _service;

    public SampleController(ISampleService service)
    {
        _service = service;
    }

    [Authorize(Policy = "ReadPolicy")]
    [HttpGet("getAllData")]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [Authorize(Policy = "ReadPolicy")]
    [HttpGet("getId/{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _service.GetAsync(id);

        if (result == null) 
            return NotFound();

        return Ok(result);
    }

    [Authorize(Policy = "WritePolicy")]
    [HttpPost("addData")]
    public async Task<IActionResult> AddAsync(SampleDTO dto)
    {
        await _service.AddAsync(dto);
        return Ok();
    }

    [Authorize(Policy = "WritePolicy")]
    [HttpPut("updateData")]
    public async Task<IActionResult> UpdateAsync(SampleDTO dto)
    {
        await _service.UpdateAsync(dto);
        return Ok();
    }

    [Authorize(Policy = "AdminPolicy")]
    [HttpDelete("deleteData/{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok();
    }
}
