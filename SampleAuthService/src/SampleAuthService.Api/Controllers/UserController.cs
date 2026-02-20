using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAuthService.Application.DTO.UserDto;
using SampleAuthService.Application.Events;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Domain.Enums;
using System.Security.Claims;

namespace SampleAuthService.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _auth;
    private readonly IEventBus _eventBus;

    public UserController(IUserService auth, IEventBus eventBus)
    {
        _auth = auth;
        _eventBus = eventBus;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> RegisterUserAsync(RegisterUserDto dto)
    {
        await _auth.RegisterUserAsync(dto);

        await _eventBus.PublishAsync(
            new UserCreatedEvent(dto.Email));

        return Ok("User Registeration Successful!");
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordRequestAsync(
        ResetPasswordDto dto)
    {
        await _auth.ResetPasswordRequestAsync(dto);

        await _eventBus.PublishAsync(
            new UserPasswordResetEvent(dto.Email));

        return Ok("Password Reset Successful!");
    }

    [Authorize(Policy = "AdminPolicy")]
    [HttpDelete("{email}")]
    public async Task<IActionResult> DeleteUserAsync(string email)
    {
        await _auth.DeleteUserAsync(email);

        await _eventBus.PublishAsync(
            new UserDeletedEvent(email));

        return Ok("User deleted Successful!");
    }

    [Authorize(Policy = "ReadPolicy")]
    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmailAsync(string email)
    {
        var currentEmail = User.FindFirstValue(ClaimTypes.Email);
        var currentRole = User.FindFirstValue(ClaimTypes.Role);

        // Admin → everything
        if (currentRole == UserRole.Admin.ToString())
        {
            var user = await _auth.GetUserByEmailAsync(email);
            return Ok(user);
        }

        // WriteUser → ReadUsers + self
        if (currentRole == UserRole.WriteUser.ToString())
        {
            var targetUser = await _auth.GetUserByEmailAsync(email);

            if (targetUser!.Role == UserRole.ReadUser ||
                string.Equals(currentEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(targetUser);
            }

            return Forbid();
        }

        // ReadUser → self only
        if (string.Equals(currentEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            var user = await _auth.GetUserByEmailAsync(email);
            return Ok(user);
        }

        return Forbid();
    }
}
