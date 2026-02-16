using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAuthService.Application.DTO;
using SampleAuthService.Application.Interfaces;

namespace SampleAuthService.Api.Controllers;

[ApiController]
[Route("api/v1/token")]
public class TokenController : ControllerBase
{
    private readonly ITokenService _authToken;

    public TokenController(ITokenService authToken)
    {
        _authToken = authToken;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> GenerateTokenAsync(TokenRequestDto dto)
    {
        var token = await _authToken.GenerateTokenAsync(dto);

        if (token == null)
            return Unauthorized();

        return Ok(token);
    }
}
