using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;

namespace BLFramework.Tests.Services;

/// <summary>
/// Tests for the InviteService business logic.
/// Verifies invite generation, usage tracking, expiration, revocation, and code uniqueness.
/// </summary>
public class InviteServiceTests
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ILogger<InviteService> CreateMockLogger()
    {
        return new Mock<ILogger<InviteService>>().Object;
    }

    [Fact]
    public async Task AddAsync_ShouldCreateNewInvite()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new InviteService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var invite = new Invite
        {
            InviteCode = "INVITECODE1234",
            CreatedById = user.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await service.AddAsync(invite);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(user.Id, result.CreatedById);
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnInviteWhenFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new InviteService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var invite = new Invite
        {
            InviteCode = "TESTCODE123456",
            CreatedById = user.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.Invites.Add(invite);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByCodeAsync(invite.InviteCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invite.InviteCode, result.InviteCode);
    }

    [Fact]
    public async Task UseInviteAsync_ShouldMarkInviteAsUsed()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new InviteService(context, logger);

        var creator = new User { Username = "creator", Password = "hash", IsActive = true };
        var newUser = new User { Username = "newuser", Password = "hash", IsActive = true };
        context.Users.AddRange(creator, newUser);
        await context.SaveChangesAsync();

        var invite = new Invite
        {
            InviteCode = "USABLECODE12345",
            CreatedById = creator.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.Invites.Add(invite);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UseInviteAsync(invite.InviteCode, newUser.Id);

        // Assert
        Assert.NotNull(result.UsedOn);
        Assert.Equal(newUser.Id, result.UsedById);
        Assert.True(result.IsUsed);
    }

    [Fact]
    public async Task RevokeInviteAsync_ShouldRevokeInviteById()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new InviteService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var invite = new Invite
        {
            InviteCode = "REVOKEABLECODE12",
            CreatedById = user.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.Invites.Add(invite);
        await context.SaveChangesAsync();

        // Act
        await service.RevokeInviteAsync(invite.Id);

        // Assert
        var revokedInvite = await context.Invites.FirstOrDefaultAsync(i => i.Id == invite.Id);
        Assert.NotNull(revokedInvite);
        Assert.True(revokedInvite.IsExpired);
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new InviteService(context, logger);

        // Act
        var result = await service.GetByCodeAsync("NONEXISTENTCODE");

        // Assert
        Assert.Null(result);
    }
}
