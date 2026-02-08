using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;

namespace BLFramework.Tests.Services;

/// <summary>
/// Tests for the UserService business logic.
/// Verifies user creation, validation, activation/deactivation, and uniqueness constraints.
/// </summary>
public class UserServiceTests
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ILogger<UserService> CreateMockLogger()
    {
        return new Mock<ILogger<UserService>>().Object;
    }

    [Fact]
    public async Task AddAsync_ShouldCreateNewUser()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new UserService(context, logger);

        var user = new User { Username = "newuser", Password = "hashedpassword", IsActive = true };

        // Act
        var result = await service.AddAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser", result.Username);
        Assert.Equal("hashedpassword", result.Password);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUserWhenExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new UserService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNullWhenNotExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new UserService(context, logger);

        // Act
        var result = await service.GetByUsernameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnTrueWhenDuplicate()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new UserService(context, logger);

        var user = new User { Username = "existinguser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UsernameExistsAsync("existinguser");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnFalseWhenUnique()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new UserService(context, logger);

        // Act
        var result = await service.UsernameExistsAsync("uniqueusername");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldSetIsActiveFalse()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new UserService(context, logger);

        var user = new User { Username = "activeuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeactivateUserAsync(user.Id);

        // Assert
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task ReactivateUserAsync_ShouldSetIsActiveTrue()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new UserService(context, logger);

        var user = new User { Username = "inactiveuser", Password = "hash", IsActive = false };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ReactivateUserAsync(user.Id);

        // Assert
        Assert.True(result.IsActive);
    }
}
