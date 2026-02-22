using FluentAssertions;
using Moq;
using SampleAuthService.Application.DTO.UserDto;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Application.Services;
using SampleAuthService.Domain.Entities;
using SampleAuthService.Domain.Enums;

namespace SampleAuthService.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly UserService _service;
    private readonly Mock<ICacheService> _cacheMock;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();
        _cacheMock = new Mock<ICacheService>();

        _service = new UserService(
            _userRepoMock.Object,
            _jwtMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Add_User_When_Email_Not_Exists()
    {
        // Arrange
        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var dto = new RegisterUserDto
        {
            Email = "new@test.com",
            Password = "password"
        };

        // Act
        await _service.RegisterUserAsync(dto);

        // Assert
        _userRepoMock.Verify(x => x.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Throw_When_Email_Exists()
    {
        // Arrange
        var existingUser = new User(
            "existing@test.com",
            BCrypt.Net.BCrypt.HashPassword("pass"),
            UserRole.ReadUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var dto = new RegisterUserDto
        {
            Email = "existing@test.com",
            Password = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.RegisterUserAsync(dto));
    }

    [Fact]
    public async Task ResetPasswordRequestAsync_Should_Update_Password_When_User_Exists()
    {
        // Arrange
        var user = new User(
            "reset@test.com",
            BCrypt.Net.BCrypt.HashPassword("old"),
            UserRole.WriteUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("reset@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var dto = new ResetPasswordDto
        {
            Email = "reset@test.com",
            NewPassword = "newpassword"
        };

        // Act
        var result = await _service.ResetPasswordRequestAsync(dto);

        // Assert
        result.Should().BeTrue();
        _userRepoMock.Verify(x => x.UpdateUserAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordRequestAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("missing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var dto = new ResetPasswordDto
        {
            Email = "missing@test.com",
            NewPassword = "pass"
        };

        // Act & Assert
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
            .Setup(x => x.GetUserByEmailAsync("delete@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepoMock
            .Setup(x => x.DeleteUserAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteUserAsync(user.Email);

        // Assert
        result.Should().BeTrue();

        _userRepoMock.Verify(x => x.DeleteUserAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task DeleteUserAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("missing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteUserAsync("missing@test.com"));

        _userRepoMock.Verify(
            x => x.DeleteUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
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
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
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
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetUserByEmailAsync(email));
    }

    [Fact]
    public async Task GetUserByEmailAsync_Should_Return_From_Cache_When_Available()
    {
        var email = "cached@test.com";

        var cachedDto = new GetUserDto
        {
            Email = email,
            Role = UserRole.ReadUser
        };

        _cacheMock
            .Setup(c => c.GetAsync<GetUserDto>($"user:email:{email}"))
            .ReturnsAsync(cachedDto);

        var result = await _service.GetUserByEmailAsync(email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);

        _userRepoMock.Verify(
            r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserByEmailAsync_Should_Fetch_From_Db_And_Set_Cache()
    {
        var email = "db@test.com";

        _cacheMock
            .Setup(c => c.GetAsync<GetUserDto>($"user:email:{email}"))
            .ReturnsAsync((GetUserDto?)null);

        var user = new User(
            email,
            BCrypt.Net.BCrypt.HashPassword("pass"),
            UserRole.ReadUser);

        _userRepoMock
            .Setup(r => r.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.GetUserByEmailAsync(email);

        result.Should().NotBeNull();

        _cacheMock.Verify(
            c => c.SetAsync(
                $"user:email:{email}",
                It.IsAny<GetUserDto>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Remove_User_Cache()
    {
        var dto = new RegisterUserDto
        {
            Email = "new@test.com",
            Password = "password"
        };

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await _service.RegisterUserAsync(dto);

        _cacheMock.Verify(
            c => c.RemoveAsync($"user:email:{dto.Email.ToLower()}"),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_Should_Remove_User_Cache()
    {
        var email = "delete@test.com";

        var user = new User(
            email,
            BCrypt.Net.BCrypt.HashPassword("pass"),
            UserRole.ReadUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _service.DeleteUserAsync(email);

        _cacheMock.Verify(
            c => c.RemoveAsync($"user:email:{email.ToLower()}"),
            Times.Once);
    }
}
