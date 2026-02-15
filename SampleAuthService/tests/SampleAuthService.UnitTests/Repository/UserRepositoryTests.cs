using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SampleAuthService.Domain.Entities;
using SampleAuthService.Domain.Enums;
using SampleAuthService.Infrastructure.Persistence;
using SampleAuthService.Infrastructure.Repositories;

namespace SampleAuthService.UnitTests.Repositories;

public class UserRepositoryTests
{
    private AuthDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Should_Add_User()
    {
        using var context = CreateDbContext();
        var repo = new UserRepository(context);

        var user = new User(
            "test@test.com",
            BCrypt.Net.BCrypt.HashPassword("password"),
            UserRole.ReadUser);

        await repo.AddAsync(user);

        context.Users.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetByEmailAsync_Should_Return_User_When_Exists()
    {
        using var context = CreateDbContext();

        var user = new User(
            "find@test.com",
            BCrypt.Net.BCrypt.HashPassword("password"),
            UserRole.ReadUser);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByEmailAsync("find@test.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("find@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_Should_Return_Null_When_Not_Exists()
    {
        using var context = CreateDbContext();
        var repo = new UserRepository(context);

        var result = await repo.GetByEmailAsync("missing@test.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_User_When_Exists()
    {
        using var context = CreateDbContext();

        var user = new User(
            "id@test.com",
            BCrypt.Net.BCrypt.HashPassword("password"),
            UserRole.ReadUser);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        var result = await repo.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_User()
    {
        using var context = CreateDbContext();

        var user = new User(
            "update@test.com",
            BCrypt.Net.BCrypt.HashPassword("oldpass"),
            UserRole.ReadUser);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        user.ChangePassword(
            BCrypt.Net.BCrypt.HashPassword("newpass"));

        await repo.UpdateAsync(user);

        var updated = await context.Users
            .FirstOrDefaultAsync(x => x.Email == "update@test.com");

        BCrypt.Net.BCrypt.Verify("newpass", updated!.PasswordHash)
            .Should().BeTrue();
    }
}
