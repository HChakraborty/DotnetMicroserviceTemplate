using FluentAssertions;
using Moq;
using SampleAuthService.Application.DTO;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Application.Services;
using SampleAuthService.Domain.Entities;
using SampleAuthService.Domain.Enums;
using Xunit;

namespace SampleAuthService.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();

        _service = new UserService(
            _userRepoMock.Object,
            _jwtMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Add_User_When_Email_Not_Exists()
    {
        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("new@test.com"))
            .ReturnsAsync((User?)null);

        var dto = new RegisterUserDto
        {
            Email = "new@test.com",
            Password = "password"
        };

        await _service.RegisterUserAsync(dto);

        _userRepoMock.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Throw_When_Email_Exists()
    {
        var existingUser = new User(
            "existing@test.com",
            BCrypt.Net.BCrypt.HashPassword("pass"),
            UserRole.ReadUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("existing@test.com"))
            .ReturnsAsync(existingUser);

        var dto = new RegisterUserDto
        {
            Email = "existing@test.com",
            Password = "password"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.RegisterUserAsync(dto));
    }

    [Fact]
    public async Task ResetPasswordRequestAsync_Should_Update_Password_When_User_Exists()
    {
        var user = new User(
            "reset@test.com",
            BCrypt.Net.BCrypt.HashPassword("old"),
            UserRole.WriteUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("reset@test.com"))
            .ReturnsAsync(user);

        var dto = new ResetPasswordDto
        {
            Email = "reset@test.com",
            NewPassword = "newpassword"
        };

        var result = await _service.ResetPasswordRequestAsync(dto);

        result.Should().BeTrue();
        _userRepoMock.Verify(x => x.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordRequestAsync_Should_Throw_When_User_Not_Found()
    {
        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("missing@test.com"))
            .ReturnsAsync((User?)null);

        var dto = new ResetPasswordDto
        {
            Email = "missing@test.com",
            NewPassword = "pass"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ResetPasswordRequestAsync(dto));
    }

    [Fact]
    public async Task DeleteUserAsync_Should_Delete_User_When_User_Exists()
    {
        // Arrange
        var user = new User(
            "delete@test.com",
            BCrypt.Net.BCrypt.HashPassword("old"),
            UserRole.WriteUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("delete@test.com"))
            .ReturnsAsync(user);

        _userRepoMock
            .Setup(x => x.DeleteUserAsync(user))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteUserAsync(user.Email);

        // Assert
        result.Should().BeTrue();

        _userRepoMock.Verify(x => x.DeleteUserAsync(user), Times.Once);
    }


    [Fact]
    public async Task DeleteUserAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("missing@test.com"))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteUserAsync("missing@test.com"));

        _userRepoMock.Verify(
            x => x.DeleteUserAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserByEmailAsync_Should_Return_Dto_When_User_Exists()
    {
        // Arrange
        var email = "user@test.com";

        var user = new User(
            email,
            BCrypt.Net.BCrypt.HashPassword("pass"),
            UserRole.ReadUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Role.Should().Be(UserRole.ReadUser);
    }

    [Fact]
    public async Task GetUserByEmailAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var email = "missing@test.com";

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetUserByEmailAsync(email));
    }
}
