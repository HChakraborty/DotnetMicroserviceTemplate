using SampleAuthService.Domain.Entities;

namespace SampleAuthService.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
