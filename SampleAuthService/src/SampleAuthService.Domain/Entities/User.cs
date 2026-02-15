using SampleAuthService.Domain.Enums;

namespace SampleAuthService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; } = UserRole.ReadUser;

    private User() { }

    public User(string email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public void ChangePassword(string hash)
    {
        PasswordHash = hash;
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
    }
}
