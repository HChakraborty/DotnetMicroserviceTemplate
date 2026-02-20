using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SampleAuthService.Application.DTO.TokenDto;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Application.Services;
using SampleAuthService.Domain.Entities;
using SampleAuthService.Domain.Enums;

namespace SampleAuthService.UnitTests.Services;

public class TokenServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly TokenService _service;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IConfigurationSection> _configSectionMock;

    public TokenServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();
        _configMock = new Mock<IConfiguration>();
        _configSectionMock = new Mock<IConfigurationSection>();

        _configSectionMock
            .Setup(x => x.Value)
            .Returns("60");

        _configMock
            .Setup(x => x.GetSection("Jwt:ExpireMinutes"))
            .Returns(_configSectionMock.Object);

        _service = new TokenService(
            _userRepoMock.Object,
            _jwtMock.Object,
            _configMock.Object);
    }

    [Fact]
    public async Task GenerateToken_Should_Return_Token_When_Valid()
    {
        // Arrange
        var password = "password123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User("test@test.com", hash, UserRole.ReadUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var tokenResponse = new TokenResponseDto
        {
            AccessToken = "fake-jwt"
        };

        _jwtMock
            .Setup(x => x.GenerateToken(user))
            .Returns("fake-jwt");

        var dto = new TokenRequestDto
        {
            Email = "test@test.com",
            Password = password
        };

        var result = await _service.GenerateTokenAsync(dto);

        result!.AccessToken.Should().Be("fake-jwt");
    }

    [Fact]
    public async Task GenerateToken_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("missing@test.com", It.IsAny<CancellationToken>()))
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
        // Arrange
        var user = new User(
            "test@test.com",
            BCrypt.Net.BCrypt.HashPassword("correct"),
            UserRole.ReadUser);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var dto = new TokenRequestDto
        {
            Email = "test@test.com",
            Password = "wrong"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateTokenAsync(dto));
    }
}
