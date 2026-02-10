using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;

namespace BLFramework.Tests.Services;

/// <summary>
/// Tests for the BaseService&lt;T&gt; generic repository service.
/// Verifies CRUD operations and error handling.
/// </summary>
public class BaseServiceTests
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ILogger<BaseService<User>> CreateMockLogger()
    {
        return new Mock<ILogger<BaseService<User>>>().Object;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new BaseService<User>(context, logger);

        var user1 = new User { Username = "user1", Password = "hash1", IsActive = true };
        var user2 = new User { Username = "user2", Password = "hash2", IsActive = true };

        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntityWhenFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new BaseService<User>(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new BaseService<User>(context, logger);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddAndPersistEntity()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new BaseService<User>(context, logger);

        var user = new User { Username = "newuser", Password = "hash", IsActive = true };

        // Act
        var result = await service.AddAsync(user);

        // Assert
        Assert.NotEqual(0, result.Id);
        var persisted = await context.Users.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal("newuser", persisted.Username);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new BaseService<User>(context, logger);

        var user = new User { Username = "original", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        user.Username = "updated";
        var result = await service.UpdateAsync(user);

        // Assert
        Assert.Equal("updated", result.Username);
        var persisted = await context.Users.FindAsync(user.Id);
        Assert.NotNull(persisted);
        Assert.Equal("updated", persisted.Username);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new BaseService<User>(context, logger);

        var user = new User { Username = "todelete", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userId = user.Id;

        // Act
        await service.DeleteAsync(user);

        // Assert
        var persisted = await context.Users.FindAsync(userId);
        Assert.Null(persisted);
    }
}
