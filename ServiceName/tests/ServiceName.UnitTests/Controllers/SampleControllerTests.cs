using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceName.Api.Controllers;
using ServiceName.Application.DTO;
using ServiceName.Application.Interfaces;

namespace ServiceName.UnitTests.Controllers;

public class SampleControllerTests
{
    private readonly Mock<ISampleService> _mockService;
    private readonly SampleController _controller;

    public SampleControllerTests()
    {
        _mockService = new Mock<ISampleService>();
        _controller = new SampleController(_mockService.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Ok_With_Data()
    {
        // Arrange
        var data = new List<SampleDTO>
        {
            new SampleDTO(),
            new SampleDTO()
        };

        _mockService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(data);

        // Act
        var result = await _controller.GetAllAsync();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsAssignableFrom<IEnumerable<SampleDTO>>(okResult.Value);

        // Assert
        Assert.Equal(2, returnedData.Count());
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Ok_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SampleDTO();

        _mockService
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetByIdAsync(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<SampleDTO>(okResult.Value);
        
        // Assert
        Assert.Equal(dto, returnedDto);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_NotFound_When_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((SampleDTO?)null);

        // Act
        var result = await _controller.GetByIdAsync(id);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddAsync_Should_Call_Service_And_Return_Ok()
    {
        // Arrange
        var dto = new SampleDTO();

        _mockService
            .Setup(x => x.AddAsync(dto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddAsync(dto);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockService.Verify(x => x.AddAsync(dto), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Call_Service_And_Return_Ok()
    {
        // Arrange
        var id = Guid.NewGuid();

        var dto = new SampleDTO
        {
            Id = id
        };

        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<SampleDTO>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(id, dto);

        // Assert
        Assert.IsType<OkResult>(result);

        _mockService.Verify(x => x.UpdateAsync(It.Is<SampleDTO>(d => d.Id == id)), Times.Once);
    }


    [Fact]
    public async Task DeleteAsync_Should_Call_Service_And_Return_Ok()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockService
            .Setup(x => x.DeleteByIdAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteByIdAsync(id);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockService.Verify(x => x.DeleteByIdAsync(id), Times.Once);
    }
}
