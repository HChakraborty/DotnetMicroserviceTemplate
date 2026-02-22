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


    // We have added DTOs for Add, Update and Delete.
    // They look same for now but in real world they can differ and it's a good practice to separate them.
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> RegisterUserAsync(RegisterUserRequestDto dto)
    {
        await _auth.RegisterUserAsync(dto);

        await _eventBus.PublishAsync(
            new UserCreatedEvent(dto.Email));

        var response = new RegisterUserResponseDto
        {
            Message = $"User Registration Successful for ${dto.Email} with Role ${dto.Role}!"
        };

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordRequestAsync(
        ResetPasswordRequestDto dto)
    {
        await _auth.ResetPasswordRequestAsync(dto);

        await _eventBus.PublishAsync(
            new UserPasswordResetEvent(dto.Email));

        var response = new ResetPasswordResponseDto
        {
            Message = $"Password Reset Successful for ${dto.Email}!"
        };

        return Ok(response);
    }

    [Authorize(Policy = "AdminPolicy")]
    [HttpDelete("{email}")]
    public async Task<IActionResult> DeleteUserAsync(string email)
    {
        await _auth.DeleteUserAsync(email);

        await _eventBus.PublishAsync(
            new UserDeletedEvent(email));

        var response = new DeleteUserResponseDto
        {
            Message = $"User Deletion Successful for ${email}!"
        };

        return Ok(response);
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
