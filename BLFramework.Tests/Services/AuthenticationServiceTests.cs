using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;

namespace BLFramework.Tests.Services;

/// <summary>
/// Tests for the AuthenticationService business logic.
/// Verifies user registration, login, password verification, and authentication workflows.
/// </summary>
public class AuthenticationServiceTests
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ILogger<UserService> CreateMockLoggerUserService()
    {
        return new Mock<ILogger<UserService>>().Object;
    }

    private ILogger<InviteService> CreateMockLoggerInviteService()
    {
        return new Mock<ILogger<InviteService>>().Object;
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldCreateNewUser()
    {
        // Arrange
        var context = CreateTestDbContext();
        var userService = new UserService(context, CreateMockLoggerUserService());
        var inviteService = new InviteService(context, CreateMockLoggerInviteService());
        var authService = new AuthenticationService(userService, inviteService);

        var username = "newuser";
        var password = "plaintext123";
        
        var creator = new User { Username = "creator", Password = "hash", IsActive = true };
        context.Users.Add(creator);
        await context.SaveChangesAsync();

        var invite = new Invite
        {
            InviteCode = "VALIDCODE1234567",
            CreatedById = creator.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.Invites.Add(invite);
        await context.SaveChangesAsync();

        // Act
        var result = await authService.RegisterUserAsync(username, password, "VALIDCODE1234567");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.NotEmpty(result.Password);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldReturnUserWhenCredentialsValid()
    {
        // Arrange
        var context = CreateTestDbContext();
        var userService = new UserService(context, CreateMockLoggerUserService());
        var inviteService = new InviteService(context, CreateMockLoggerInviteService());
        var authService = new AuthenticationService(userService, inviteService);

        var username = "testuser";
        var password = "testpassword123";

        var creator = new User { Username = "creator", Password = "hash", IsActive = true };
        context.Users.Add(creator);
        await context.SaveChangesAsync();

        var invite = new Invite
        {
            InviteCode = "TESTCODE1234567",
            CreatedById = creator.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.Invites.Add(invite);
        await context.SaveChangesAsync();

        var user = await authService.RegisterUserAsync(username, password, "TESTCODE1234567");

        // Act
        var result = await authService.AuthenticateUserAsync(username, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldReturnNullWhenUserNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var userService = new UserService(context, CreateMockLoggerUserService());
        var inviteService = new InviteService(context, CreateMockLoggerInviteService());
        var authService = new AuthenticationService(userService, inviteService);

        // Act
        var result = await authService.AuthenticateUserAsync("nonexistent", "password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldReturnNullWhenPasswordInvalid()
    {
        // Arrange
        var context = CreateTestDbContext();
        var userService = new UserService(context, CreateMockLoggerUserService());
        var inviteService = new InviteService(context, CreateMockLoggerInviteService());
        var authService = new AuthenticationService(userService, inviteService);

        var username = "testuser";
        var password = "testpassword123";
        var wrongPassword = "wrongpassword";

        var creator = new User { Username = "creator", Password = "hash", IsActive = true };
        context.Users.Add(creator);
        await context.SaveChangesAsync();

        var invite = new Invite
        {
            InviteCode = "AUTHCODE1234567",
            CreatedById = creator.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.Invites.Add(invite);
        await context.SaveChangesAsync();

        await authService.RegisterUserAsync(username, password, "AUTHCODE1234567");

        // Act
        var result = await authService.AuthenticateUserAsync(username, wrongPassword);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldReturnNullWhenUserInactive()
    {
        // Arrange
        var context = CreateTestDbContext();
        var userService = new UserService(context, CreateMockLoggerUserService());
        var inviteService = new InviteService(context, CreateMockLoggerInviteService());
        var authService = new AuthenticationService(userService, inviteService);

        var username = "testuser";
        var password = "testpassword123";

        var creator = new User { Username = "creator", Password = "hash", IsActive = true };
        context.Users.Add(creator);
        await context.SaveChangesAsync();

        var invite = new Invite
        {
            InviteCode = "INACTIVECODE12345",
            CreatedById = creator.Id,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        context.Invites.Add(invite);
        await context.SaveChangesAsync();

        var user = await authService.RegisterUserAsync(username, password, "INACTIVECODE12345");
        
        // Deactivate user
        await userService.DeactivateUserAsync(user.Id);

        // Act
        var result = await authService.AuthenticateUserAsync(username, password);

        // Assert
        Assert.Null(result);
    }
}
