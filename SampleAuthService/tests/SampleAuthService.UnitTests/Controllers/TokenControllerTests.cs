using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SampleAuthService.Api.Controllers;
using SampleAuthService.Application.DTO;
using SampleAuthService.Application.Interfaces;

namespace SampleAuthService.UnitTests.Controllers;

public class TokenControllerTests
{
    private readonly Mock<ITokenService> _tokenMock;
    private readonly TokenController _controller;

    public TokenControllerTests()
    {
        _tokenMock = new Mock<ITokenService>();
        _controller = new TokenController(_tokenMock.Object);
    }

    [Fact]
    public async Task GenerateTokenAsyn_Should_Return_Ok_With_Token_When_Valid()
    {
        // Arrange
        var dto = new TokenRequestDto
        {
            Email = "test@test.com",
            Password = "password"
        };

        var tokenResponse = new TokenResponseDto
        {
            AccessToken = "fake-jwt"
        };

        _tokenMock
            .Setup(x => x.GenerateTokenAsync(dto))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _controller.GenerateTokenAsync(dto);

        // Assert
        var okResult = result as OkObjectResult;

        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as TokenResponseDto;

        response.Should().NotBeNull();
        response!.AccessToken.Should().Be("fake-jwt");
    }


    [Fact]
    public async Task GenerateTokenAsyn_Should_Return_Unauthorized_When_Invalid()
    {
        // Arrange
        var dto = new TokenRequestDto
        {
            Email = "bad@test.com",
            Password = "wrong"
        };

        _tokenMock
            .Setup(x => x.GenerateTokenAsync(dto))
            .ReturnsAsync((TokenResponseDto?)null);

        // Act
        var result = await _controller.GenerateTokenAsync(dto);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }
}
