using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SampleAuthService.Application.DTO;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Controllers;
using System.Security.Claims;

namespace SampleAuthService.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authMock = new Mock<IAuthService>();
        _controller = new AuthController(_authMock.Object);
    }

    [Fact]
    public async Task Token_Should_Return_Ok_When_Valid()
    {
        var dto = new TokenRequestDto
        {
            Email = "test@test.com",
            Password = "password"
        };

        _authMock
            .Setup(x => x.GenerateTokenAsync(dto))
            .ReturnsAsync("fake-jwt");

        var result = await _controller.Token(dto);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Token_Should_Return_Unauthorized_When_Invalid()
    {
        var dto = new TokenRequestDto
        {
            Email = "bad@test.com",
            Password = "wrong"
        };

        _authMock
            .Setup(x => x.GenerateTokenAsync(dto))
            .ReturnsAsync((string?)null);

        var result = await _controller.Token(dto);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Register_Should_Return_Ok()
    {
        var dto = new RegisterDto
        {
            Email = "new@test.com",
            Password = "password"
        };

        var result = await _controller.Register(dto);

        result.Should().BeOfType<OkObjectResult>();

        _authMock.Verify(x => x.RegisterAsync(dto), Times.Once);
    }

    [Fact]
    public void Me_Should_Return_User_Claims()
    {
        var claims = new List<Claim>
        {
            new Claim("sub", "123"),
            new Claim(ClaimTypes.Email, "user@test.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        var result = _controller.Me();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ResetRequest_Should_Return_Ok()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@test.com",
            NewPassword = "newpass"
        };

        var result = await _controller.ResetRequest(dto);

        result.Should().BeOfType<OkObjectResult>();

        _authMock.Verify(x => x.ResetPasswordAsync(dto), Times.Once);
    }
}
