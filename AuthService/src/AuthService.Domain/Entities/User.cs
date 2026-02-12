namespace SampleAuthService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public string Role { get; private set; } = "User";

    private User() { }

    public User(string email, string passwordHash, string role = "User")
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

    public void ChangeRole(string role)
    {
        Role = role;
    }
}
