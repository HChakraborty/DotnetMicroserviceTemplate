using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAuthService.Application.DTO;
using SampleAuthService.Application.Interfaces;

namespace SampleAuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<IActionResult> Token(TokenRequestDto dto)
    {
        var token = await _auth.GenerateTokenAsync(dto);

        if (token == null)
            return Unauthorized();

        return Ok(new { accessToken = token });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        await _auth.RegisterAsync(dto);

        return Ok("User Registeration Successful!");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Id = User.FindFirst("sub")?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    [AllowAnonymous]
    [HttpPost("password/reset-request")]
    public async Task<IActionResult> ResetRequest(
        ResetPasswordDto dto)
    {
        await _auth.ResetPasswordAsync(dto);

        return Ok("Password Reset Successful!");
    }
}
