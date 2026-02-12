using FluentAssertions;
using Moq;
using SampleAuthService.Application.DTO;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Application.Services;
using SampleAuthService.Domain.Entities;
using Xunit;

namespace SampleAuthService.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();

        _service = new AuthService(
            _userRepoMock.Object,
            _jwtMock.Object);
    }

    [Fact]
    public async Task GenerateToken_Should_Return_Token_When_Valid()
    {
        var password = "password123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User("test@test.com", hash, "User");

        _userRepoMock
            .Setup(x => x.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(user);

        _jwtMock
            .Setup(x => x.GenerateToken(user.Id, user.Email, user.Role))
            .Returns("fake-jwt");

        var dto = new TokenRequestDto
        {
            Email = "test@test.com",
            Password = password
        };

        var result = await _service.GenerateTokenAsync(dto);

        result.Should().Be("fake-jwt");
    }

    [Fact]
    public async Task GenerateToken_Should_Throw_When_User_Not_Found()
    {
        _userRepoMock
            .Setup(x => x.GetByEmailAsync("missing@test.com"))
            .ReturnsAsync((User?)null);

        var dto = new TokenRequestDto
        {
            Email = "missing@test.com",
            Password = "pass"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GenerateTokenAsync(dto));
    }

    [Fact]
    public async Task GenerateToken_Should_Throw_When_Invalid_Password()
    {
        var user = new User(
            "test@test.com",
            BCrypt.Net.BCrypt.HashPassword("correct"),
            "User");

        _userRepoMock
            .Setup(x => x.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(user);

        var dto = new TokenRequestDto
        {
            Email = "test@test.com",
            Password = "wrong"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateTokenAsync(dto));
    }

    [Fact]
    public async Task Register_Should_Add_User_When_Email_Not_Exists()
    {
        _userRepoMock
            .Setup(x => x.GetByEmailAsync("new@test.com"))
            .ReturnsAsync((User?)null);

        var dto = new RegisterDto
        {
            Email = "new@test.com",
            Password = "password"
        };

        await _service.RegisterAsync(dto);

        _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Register_Should_Throw_When_Email_Exists()
    {
        var existingUser = new User(
            "existing@test.com",
            BCrypt.Net.BCrypt.HashPassword("pass"),
            "User");

        _userRepoMock
            .Setup(x => x.GetByEmailAsync("existing@test.com"))
            .ReturnsAsync(existingUser);

        var dto = new RegisterDto
        {
            Email = "existing@test.com",
            Password = "password"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));
    }

    [Fact]
    public async Task ResetPassword_Should_Update_Password_When_User_Exists()
    {
        var user = new User(
            "reset@test.com",
            BCrypt.Net.BCrypt.HashPassword("old"),
            "User");

        _userRepoMock
            .Setup(x => x.GetByEmailAsync("reset@test.com"))
            .ReturnsAsync(user);

        var dto = new ResetPasswordDto
        {
            Email = "reset@test.com",
            NewPassword = "newpassword"
        };

        var result = await _service.ResetPasswordAsync(dto);

        result.Should().BeTrue();
        _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_Should_Throw_When_User_Not_Found()
    {
        _userRepoMock
            .Setup(x => x.GetByEmailAsync("missing@test.com"))
            .ReturnsAsync((User?)null);

        var dto = new ResetPasswordDto
        {
            Email = "missing@test.com",
            NewPassword = "pass"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ResetPasswordAsync(dto));
    }
}
