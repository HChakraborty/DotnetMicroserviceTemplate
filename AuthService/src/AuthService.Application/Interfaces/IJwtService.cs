namespace SampleAuthService.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid id, string email, string role);
}
