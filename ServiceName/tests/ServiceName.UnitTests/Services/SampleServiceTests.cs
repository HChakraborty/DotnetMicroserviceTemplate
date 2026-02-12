using Moq;
using ServiceName.Application.DTO;
using ServiceName.Application.Interfaces;
using ServiceName.Application.Services;
using ServiceName.Domain.Entities;

namespace ServiceName.UnitTests.Services;

public class SampleServiceTests
{
    private readonly Mock<IRepository<SampleEntity>> _repoMock;
    private readonly ISampleService _service;

    public SampleServiceTests()
    {
        _repoMock = new Mock<IRepository<SampleEntity>>();

        _service = new SampleService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Returns_Mapped_Dtos()
    {
        // Arrange
        var entities = new List<SampleEntity>
        {
            new() { Id = Guid.NewGuid(), name = "One" },
            new() { Id = Guid.NewGuid(), name = "Two" }
        };

        _repoMock
            .Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(entities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("One", result[0].Name);
        Assert.Equal("Two", result[1].Name);
    }

    [Fact]
    public async Task GetAsync_Returns_Dto_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        var entity = new SampleEntity
        {
            Id = id,
            name = "Test"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.GetAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetAsync_Returns_Null_When_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync((SampleEntity?)null);

        // Act
        var result = await _service.GetAsync(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_Creates_Entity_And_Calls_Repository()
    {
        // Arrange
        var dto = new SampleDTO
        {
            Name = "New Item"
        };

        SampleEntity? savedEntity = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<SampleEntity>(), default))
            .Callback<SampleEntity, CancellationToken>((e, _) =>
            {
                savedEntity = e;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddAsync(dto);

        // Assert
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<SampleEntity>(), default),
            Times.Once);

        Assert.NotNull(savedEntity);
        Assert.Equal("New Item", savedEntity!.name);
        Assert.NotEqual(Guid.Empty, savedEntity.Id);
    }

    [Fact]
    public async Task UpdateAsync_Calls_Repository_With_Mapped_Entity()
    {
        // Arrange
        var id = Guid.NewGuid();

        var dto = new SampleDTO
        {
            Id = id,
            Name = "Updated"
        };

        SampleEntity? updated = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<SampleEntity>(), default))
            .Callback<SampleEntity, CancellationToken>((e, _) =>
            {
                updated = e;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(dto);

        // Assert
        _repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<SampleEntity>(), default),
            Times.Once);

        Assert.NotNull(updated);
        Assert.Equal(id, updated!.Id);
        Assert.Equal("Updated", updated.name);
    }

    [Fact]
    public async Task DeleteAsync_Calls_Repository()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.DeleteByIdAsync(id, default))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _repoMock.Verify(
            r => r.DeleteByIdAsync(id, default),
            Times.Once);
    }
}
