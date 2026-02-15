using SampleAuthService.Application.DTO;

namespace SampleAuthService.Application.Interfaces;

public interface IAuthService
{
    Task<string?> GenerateTokenAsync(TokenRequestDto dto);

    Task RegisterAsync(RegisterDto dto);

    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
}
