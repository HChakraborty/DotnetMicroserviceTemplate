using SampleAuthService.Application.DTO.TokenDto;
using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Interfaces;

public interface ITokenService
{
    Task<TokenResponseDto?> GenerateTokenAsync(TokenRequestDto dto);
}
