using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ServiceName.Application.Interfaces;
using ServiceName.Domain.Entities;
using ServiceName.Infrastructure.Persistence;
using ServiceName.Infrastructure.Repositories;

namespace ServiceName.UnitTests.Repositories;

public class SampleRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private IRepository<SampleEntity> _repo = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);

        await _context.Database.EnsureCreatedAsync();

        _repo = new SampleRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_Should_Save_Entity()
    {
        // Arrange
        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Name"
        };

        await _repo.AddAsync(entity);

        // Act
        var saved = await _context.SampleEntities.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(saved);
        Assert.Equal("Test Name", saved!.Name);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Entities()
    {
        // Arrange
        _context.SampleEntities.AddRange(
            new SampleEntity { Id = Guid.NewGuid(), Name = "One" },
            new SampleEntity { Id = Guid.NewGuid(), Name = "Two" }
        );

        await _context.SaveChangesAsync();

        // Act
        var result = await _repo.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Entity_When_Found()
    {
        // Arrange
        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Find Me"
        };

        _context.SampleEntities.Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repo.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Find Me", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _repo.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Entity()
    {
        // Arrange
        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Old Name"
        };

        _context.SampleEntities.Add(entity);
        await _context.SaveChangesAsync();

        entity.Name = "New Name";

        await _repo.UpdateAsync(entity);

        // Act
        var updated = await _context.SampleEntities.FirstAsync();

        // Assert
        Assert.Equal("New Name", updated.Name);
    }

    [Fact]
    public async Task DeleteByIdAsync_Should_Remove_Entity_When_Found()
    {
        // Arrange
        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Delete Me"
        };

        _context.SampleEntities.Add(entity);
        await _context.SaveChangesAsync();

        await _repo.DeleteByIdAsync(entity.Id);

        // Act
        var deleted = await _context.SampleEntities.FirstOrDefaultAsync();

        // Assert
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteByIdAsync_Should_Do_Nothing_When_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        await _repo.DeleteByIdAsync(id);

        // Act
        var count = await _context.SampleEntities.CountAsync();

        // Assert
        Assert.Equal(0, count);
    }
}
