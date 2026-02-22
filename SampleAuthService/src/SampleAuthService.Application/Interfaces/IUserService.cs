using SampleAuthService.Application.DTO.UserDto;
using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Interfaces;

public interface IUserService
{
    Task RegisterUserAsync(RegisterUserDto dto);
    Task<bool> ResetPasswordRequestAsync(ResetPasswordDto dto);
    Task<bool> DeleteUserAsync(string email);
    Task<GetUserRequestDto?> GetUserByEmailAsync(string email);
}
