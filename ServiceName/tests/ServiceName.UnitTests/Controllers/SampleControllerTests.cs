using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceName.Api.Controllers;
using ServiceName.Application.DTO;
using ServiceName.Application.Events;
using ServiceName.Application.Interfaces;

namespace ServiceName.UnitTests.Controllers;

public class SampleControllerTests
{
    private readonly Mock<ISampleService> _mockService;
    private readonly SampleController _controller;
    private readonly Mock<IEventBus> _eventBus;

    public SampleControllerTests()
    {
        _mockService = new Mock<ISampleService>();
        _eventBus = new Mock<IEventBus>();
        _controller = new SampleController(_mockService.Object, _eventBus.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Ok_With_Data()
    {
        // Arrange
        var data = new List<GetSampleRequestDto>
        {
            new GetSampleRequestDto(),
            new GetSampleRequestDto()
        };

        _mockService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(data);

        // Act
        var result = await _controller.GetAllAsync();

        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsAssignableFrom<IEnumerable<GetSampleRequestDto>>(okObjectResult.Value);

        // Assert
        Assert.Equal(2, returnedData.Count());
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Ok_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new GetSampleRequestDto();

        _mockService
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetByIdAsync(id);

        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<GetSampleRequestDto>(okObjectResult.Value);
        
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
            .ReturnsAsync((GetSampleRequestDto?)null);

        // Act
        var result = await _controller.GetByIdAsync(id);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddAsync_Should_Call_Service_And_Return_Ok()
    {
        // Arrange
        var dto = new AddSampleRequestDto();

        _mockService
            .Setup(x => x.AddAsync(dto))
            .Returns(Task.FromResult(Guid.NewGuid()));

        // Act
        var result = await _controller.AddAsync(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(x => x.AddAsync(dto), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Call_Service_And_Return_Ok()
    {
        // Arrange
        var id = Guid.NewGuid();

        var dto = new UpdateSampleRequestDto
        {
            Id = id,
            Name = "Updated Name"
        };

        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateSampleRequestDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(id, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        _mockService.Verify(x => x.UpdateAsync(It.Is<UpdateSampleRequestDto>(d => d.Id == id)), Times.Once);
        _mockService.Verify(x => x.UpdateAsync(It.Is<UpdateSampleRequestDto>(d => d.Name == dto.Name)), Times.Once);
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
        Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(x => x.DeleteByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AddAsync_Should_Publish_SampleCreatedEvent()
    {
        var dto = new AddSampleRequestDto
        {
            Name = "New Sample"
        };

        var createdId = Guid.NewGuid();

        _mockService
            .Setup(x => x.AddAsync(dto))
            .ReturnsAsync(createdId);

        var result = await _controller.AddAsync(dto);

        Assert.IsType<OkObjectResult>(result);

        _eventBus.Verify(
            e => e.PublishAsync(It.Is<SampleCreatedEvent>(
                ev => ev.Id == createdId)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Publish_SampleUpdatedEvent()
    {
        var id = Guid.NewGuid();

        var dto = new UpdateSampleRequestDto
        {
            Id = id,
            Name = "Updated"
        };

        var result = await _controller.UpdateAsync(id, dto);

        Assert.IsType<OkObjectResult>(result);

        _eventBus.Verify(
            e => e.PublishAsync(It.Is<SampleUpdatedEvent>(
                ev => ev.Id == id)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Publish_SampleDeletedEvent_When_Entity_Exists()
    {
        var id = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(new GetSampleRequestDto { Id = id });

        var result = await _controller.DeleteByIdAsync(id);

        Assert.IsType<OkObjectResult>(result);

        _eventBus.Verify(
            e => e.PublishAsync(It.Is<SampleDeletedEvent>(
                ev => ev.Id == id)),
            Times.Once);
    }
}
