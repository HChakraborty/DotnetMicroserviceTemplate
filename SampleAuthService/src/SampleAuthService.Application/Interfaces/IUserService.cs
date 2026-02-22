using SampleAuthService.Application.DTO.UserDto;
using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Interfaces;

public interface IUserService
{
    Task RegisterUserAsync(RegisterUserRequestDto dto);
    Task<bool> ResetPasswordRequestAsync(ResetPasswordRequestDto dto);
    Task<bool> DeleteUserAsync(string email);
    Task<GetUserRequestDto?> GetUserByEmailAsync(string email);
}
