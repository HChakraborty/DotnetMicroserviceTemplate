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
    private readonly Mock<ICacheService> _cacheMock;

    public SampleServiceTests()
    {
        _repoMock = new Mock<IRepository<SampleEntity>>();
        _cacheMock = new Mock<ICacheService>();
        _service = new SampleService(_repoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Returns_Mapped_Dtos()
    {
        // Arrange
        var entities = new List<SampleEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "One" },
            new() { Id = Guid.NewGuid(), Name = "Two" }
        };

        _repoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
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
            Name = "Test"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(id);

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
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SampleEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_Creates_Entity_And_Calls_Repository()
    {
        // Arrange
        var dto = new AddSampleRequestDto
        {
            Name = "New Item"
        };

        SampleEntity? savedEntity = null;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<SampleEntity>(), It.IsAny<CancellationToken>()))
            .Callback<SampleEntity, CancellationToken>((e, _) =>
            {
                savedEntity = e;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddAsync(dto);

        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<SampleEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Assert
        Assert.NotNull(savedEntity);
        Assert.Equal("New Item", savedEntity!.Name);
        Assert.NotEqual(Guid.Empty, savedEntity.Id);
    }

    [Fact]
    public async Task UpdateAsync_Calls_Repository_With_Mapped_Entity()
    {
        // Arrange
        var id = Guid.NewGuid();

        var dto = new UpdateSampleRequestDto
        {
            Id = id,
            Name = "Updated"
        };

        SampleEntity? updated = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<SampleEntity>(), It.IsAny<CancellationToken>()))
            .Callback<SampleEntity, CancellationToken>((e, _) =>
            {
                updated = e;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(dto);

        _repoMock.Verify(
            r => r.UpdateAsync(It.IsAny<SampleEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(id, updated!.Id);
        Assert.Equal("Updated", updated.Name);
    }

    [Fact]
    public async Task DeleteAsync_Calls_Repository()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.DeleteByIdAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteByIdAsync(id);

        // Assert
        _repoMock.Verify(
            r => r.DeleteByIdAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_From_Cache_When_Available()
    {
        var id = Guid.NewGuid();

        var cachedDto = new GetSampleRequestDto
        {
            Id = id,
            Name = "Cached"
        };

        _cacheMock
            .Setup(c => c.GetAsync<GetSampleRequestDto>($"sample:id:{id}"))
            .ReturnsAsync(cachedDto);

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal("Cached", result!.Name);

        // Repository should NOT be called
        _repoMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Fetch_From_Db_And_Set_Cache_When_Not_In_Cache()
    {
        var id = Guid.NewGuid();

        _cacheMock
            .Setup(c => c.GetAsync<GetSampleRequestDto>($"sample:id:{id}"))
            .ReturnsAsync((GetSampleRequestDto?)null);

        var entity = new SampleEntity
        {
            Id = id,
            Name = "From DB"
        };

        _repoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal("From DB", result!.Name);

        _cacheMock.Verify(
            c => c.SetAsync(
                $"sample:id:{id}",
                It.IsAny<GetSampleRequestDto>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task AddAsync_Should_Remove_Item_Cache()
    {
        var dto = new AddSampleRequestDto
        {
            Name = "New Item"
        };

        Guid createdId = Guid.Empty;

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<SampleEntity>(), It.IsAny<CancellationToken>()))
            .Callback<SampleEntity, CancellationToken>((e, _) =>
            {
                createdId = e.Id;
            })
            .Returns(Task.CompletedTask);

        await _service.AddAsync(dto);

        _cacheMock.Verify(
            c => c.RemoveAsync($"sample:id:{createdId}"),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Remove_Item_Cache()
    {
        var id = Guid.NewGuid();

        var dto = new UpdateSampleRequestDto
        {
            Id = id,
            Name = "Updated"
        };

        await _service.UpdateAsync(dto);

        _cacheMock.Verify(
            c => c.RemoveAsync($"sample:id:{id}"),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Item_Cache()
    {
        var id = Guid.NewGuid();

        await _service.DeleteByIdAsync(id);

        _cacheMock.Verify(
            c => c.RemoveAsync($"sample:id:{id}"),
            Times.Once);
    }
}
