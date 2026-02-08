using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;

namespace BLFramework.Tests.Services;

/// <summary>
/// Tests for the RefreshTokenService business logic.
/// Verifies token generation, validation, revocation, and cleanup operations.
/// </summary>
public class RefreshTokenServiceTests
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ILogger<BaseService<RefreshToken>> CreateMockLogger()
    {
        return new Mock<ILogger<BaseService<RefreshToken>>>().Object;
    }

    [Fact]
    public async Task AddAsync_ShouldCreateNewToken()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new RefreshTokenService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = new RefreshToken
        {
            Token = "newtoken123",
            UserId = user.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await service.AddAsync(token);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldReturnTokenWhenFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new RefreshTokenService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = new RefreshToken
        {
            Token = "testtoken",
            UserId = user.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        // Act
        var isValid = await service.GetByTokenAsync(token.Token);

        // Assert
        Assert.NotNull(isValid);
        Assert.Equal(token.Token, isValid.Token);
    }

    [Fact]
    public async Task TokenExistsAsync_ShouldReturnFalseForExpiredToken()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new RefreshTokenService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = new RefreshToken
        {
            Token = "expiredtoken",
            UserId = user.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(-1)
        };
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        // Act
        var exists = await service.TokenExistsAsync(token.Token);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task RevokeTokenByStringAsync_ShouldSetRevokedOn()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new RefreshTokenService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = new RefreshToken
        {
            Token = "tokentrevoke",
            UserId = user.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        // Act
        await service.RevokeTokenByStringAsync(token.Token);

        // Assert
        var revokedToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token.Token);
        Assert.NotNull(revokedToken);
        Assert.NotNull(revokedToken.RevokedOn);
        Assert.False(revokedToken.IsActive);
    }

    [Fact]
    public async Task GetTokensForUserAsync_ShouldReturnUserTokens()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new RefreshTokenService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = new RefreshToken
        {
            Token = "usertoken",
            UserId = user.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTokensForUserAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Single(result);
    }
}
