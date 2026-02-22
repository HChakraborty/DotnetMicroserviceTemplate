using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SampleAuthService.Api.Controllers;
using SampleAuthService.Application.DTO;
using SampleAuthService.Application.DTO.UserDto;
using SampleAuthService.Application.Events;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Domain.Enums;
using System.Security.Claims;

namespace SampleAuthService.UnitTests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userMock;
    private readonly UserController _controller;
    private readonly Mock<IEventBus> _eventBus;

    public UserControllerTests()
    {
        _userMock = new Mock<IUserService>();
        _eventBus = new Mock<IEventBus>();
        _controller = new UserController(_userMock.Object, _eventBus.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Return_Ok()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "new@test.com",
            Password = "password"
        };

        _userMock
            .Setup(x => x.RegisterUserAsync(dto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RegisterUserAsync(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _userMock.Verify(x => x.RegisterUserAsync(dto), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordRequestAsync_Should_Return_Ok()
    {
        // Arrange
        var dto = new ResetPasswordDto
        {
            Email = "user@test.com",
            NewPassword = "newpass"
        };

        _userMock
            .Setup(x => x.ResetPasswordRequestAsync(dto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetPasswordRequestAsync(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _userMock.Verify(x => x.ResetPasswordRequestAsync(dto), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_Should_Return_Ok_When_Service_Succeeds()
    {
        // Arrange
        var email = "user@test.com";

        _userMock
            .Setup(x => x.DeleteUserAsync(email))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUserAsync(email);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _userMock.Verify(x => x.DeleteUserAsync(email), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_Should_Throw_When_Service_Fails()
    {
        // Arrange
        var email = "missing@test.com";

        _userMock
            .Setup(x => x.DeleteUserAsync(email))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.DeleteUserAsync(email));
    }

    [Fact]
    public async Task GetUserAsync_Should_Return_User_When_WriteUser_Accesses_ReadUser()
    {
        // Arrange
        var targetEmail = "read@test.com";

        var dto = new GetUserDto
        {
            Email = targetEmail,
            Role = UserRole.ReadUser
        };

        _userMock
            .Setup(x => x.GetUserByEmailAsync(targetEmail))
            .ReturnsAsync(dto);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "writer@test.com"),
            new Claim(ClaimTypes.Role, UserRole.WriteUser.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetUserByEmailAsync(targetEmail);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;

        var returned = ok.Value.Should().BeOfType<GetUserDto>().Subject;

        returned.Email.Should().Be(targetEmail);
    }

    [Fact]
    public async Task GetUserAsync_Should_Return_Forbid_When_ReadUser_Accesses_Other_User()
    {
        // Arrange
        var targetEmail = "other@test.com";

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "read@test.com"),
            new Claim(ClaimTypes.Role, UserRole.ReadUser.ToString())
        },  "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetUserByEmailAsync(targetEmail);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetUserAsync_Should_Return_Forbid_When_WriteUser_Accesses_Admin()
    {
        // Arrange
        var targetEmail = "admin@test.com";

        var adminDto = new GetUserDto
        {
            Email = targetEmail,
            Role = UserRole.Admin
        };

        _userMock
            .Setup(x => x.GetUserByEmailAsync(targetEmail))
            .ReturnsAsync(adminDto);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.Email, "writer@test.com"),
        new Claim(ClaimTypes.Role, UserRole.WriteUser.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetUserByEmailAsync(targetEmail);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetUserAsync_Should_Return_Unauthorized_When_No_User()
    {
        // Arrange
        var targetEmail = "read@test.com";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // no user
        };

        // Act
        var result = await _controller.GetUserByEmailAsync(targetEmail);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Publish_UserCreatedEvent()
    {
        var dto = new RegisterUserDto
        {
            Email = "new@test.com",
            Password = "password"
        };

        _userMock
            .Setup(x => x.RegisterUserAsync(dto))
            .Returns(Task.CompletedTask);

        var result = await _controller.RegisterUserAsync(dto);

        result.Should().BeOfType<OkObjectResult>();

        _eventBus.Verify(
            e => e.PublishAsync(It.Is<UserCreatedEvent>(
                ev => ev.Email == dto.Email)),
            Times.Once);
    }

    [Fact]
    public async Task ResetPasswordRequestAsync_Should_Publish_UserPasswordResetEvent()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@test.com",
            NewPassword = "newpass"
        };

        _userMock
            .Setup(x => x.ResetPasswordRequestAsync(dto))
            .ReturnsAsync(true);

        var result = await _controller.ResetPasswordRequestAsync(dto);

        result.Should().BeOfType<OkObjectResult>();

        _eventBus.Verify(
            e => e.PublishAsync(It.Is<UserPasswordResetEvent>(
                ev => ev.Email == dto.Email)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_Should_Publish_UserDeletedEvent()
    {
        var email = "user@test.com";

        _userMock
            .Setup(x => x.DeleteUserAsync(email))
            .ReturnsAsync(true);

        var result = await _controller.DeleteUserAsync(email);

        result.Should().BeOfType<OkObjectResult>();

        _eventBus.Verify(
            e => e.PublishAsync(It.Is<UserDeletedEvent>(
                ev => ev.Email == email)),
            Times.Once);
    }
}
