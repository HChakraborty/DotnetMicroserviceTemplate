using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ServiceName.Domain.Entities;
using ServiceName.Application.Interfaces;
using ServiceName.Infrastructure.Persistence;
using ServiceName.Infrastructure.Repositories;


namespace ServiceName.UnitTests.Repositories;

public class SampleRepositoryTests
{
    private static AppDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }

    private static IRepository<SampleEntity> CreateRepository(AppDbContext context)
    {
        return new SampleRepository(context);
    }

    [Fact]
    public async Task AddAsync_Should_Save_Entity()
    {
        // Arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            name = "Test Name"
        };

        // Act
        await repo.AddAsync(entity);

        // Assert
        var saved = await context.SampleEntities.FirstOrDefaultAsync();

        Assert.NotNull(saved);
        Assert.Equal("Test Name", saved!.name);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Entities()
    {
        // Arrange
        using var context = CreateDbContext();

        context.SampleEntities.AddRange(
            new SampleEntity { Id = Guid.NewGuid(), name = "One" },
            new SampleEntity { Id = Guid.NewGuid(), name = "Two" }
        );

        await context.SaveChangesAsync();

        var repo = CreateRepository(context);

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Entity_When_Found()
    {
        // Arrange
        using var context = CreateDbContext();

        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            name = "Find Me"
        };

        context.SampleEntities.Add(entity);
        await context.SaveChangesAsync();

        var repo = CreateRepository(context);

        // Act
        var result = await repo.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Find Me", result!.name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        using var context = CreateDbContext();

        var repo = CreateRepository(context);

        // Act
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Entity()
    {
        // Arrange
        using var context = CreateDbContext();

        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            name = "Old Name"
        };

        context.SampleEntities.Add(entity);
        await context.SaveChangesAsync();

        var repo = CreateRepository(context);

        entity.name = "New Name";

        // Act
        await repo.UpdateAsync(entity);

        // Assert
        var updated = await context.SampleEntities.FirstAsync();

        Assert.Equal("New Name", updated.name);
    }

    [Fact]
    public async Task DeleteByIdAsync_Should_Remove_Entity_When_Found()
    {
        // Arrange
        using var context = CreateDbContext();

        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            name = "Delete Me"
        };

        context.SampleEntities.Add(entity);
        await context.SaveChangesAsync();

        var repo = CreateRepository(context);

        // Act
        await repo.DeleteByIdAsync(entity.Id);

        // Assert
        var deleted = await context.SampleEntities.FirstOrDefaultAsync();

        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteByIdAsync_Should_Do_Nothing_When_Not_Found()
    {
        // Arrange
        using var context = CreateDbContext();

        var repo = CreateRepository(context);

        // Act
        await repo.DeleteByIdAsync(Guid.NewGuid());

        // Assert
        var count = await context.SampleEntities.CountAsync();

        Assert.Equal(0, count);
    }
}
