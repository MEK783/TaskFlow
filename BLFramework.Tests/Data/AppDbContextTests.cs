using Xunit;
using Microsoft.EntityFrameworkCore;
using BLFramework.Data;
using BLFramework.Models;

namespace BLFramework.Tests.Data;

/// <summary>
/// Tests for the AppDbContext Entity Framework Core configuration.
/// Verifies entity mappings, relationships, and database constraints.
/// </summary>
public class AppDbContextTests
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void DbContext_ShouldHaveUserDbSet()
    {
        // Arrange & Act
        var context = CreateTestDbContext();

        // Assert
        Assert.NotNull(context.Users);
    }

    [Fact]
    public void DbContext_ShouldHaveUserTaskDbSet()
    {
        // Arrange & Act
        var context = CreateTestDbContext();

        // Assert
        Assert.NotNull(context.Tasks);
    }

    [Fact]
    public void DbContext_ShouldHaveTaskStatusDefinitionDbSet()
    {
        // Arrange & Act
        var context = CreateTestDbContext();

        // Assert
        Assert.NotNull(context.TaskStatusDefinitions);
    }

    [Fact]
    public void DbContext_ShouldHaveRefreshTokenDbSet()
    {
        // Arrange & Act
        var context = CreateTestDbContext();

        // Assert
        Assert.NotNull(context.RefreshTokens);
    }

    [Fact]
    public void DbContext_ShouldHaveInviteDbSet()
    {
        // Arrange & Act
        var context = CreateTestDbContext();

        // Assert
        Assert.NotNull(context.Invites);
    }

    [Fact]
    public async Task DbContext_ShouldSaveChanges()
    {
        // Arrange
        var context = CreateTestDbContext();
        var user = new User { Username = "testuser", Password = "hash", IsActive = true };

        // Act
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Assert
        Assert.NotEqual(0, user.Id);
    }

    [Fact]
    public async Task DbContext_UserTaskShouldHaveNavigationToUser()
    {
        // Arrange
        var context = CreateTestDbContext();
        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        context.Users.Add(user);
        context.TaskStatusDefinitions.Add(status);
        await context.SaveChangesAsync();

        var task = new UserTask { TaskName = "Task", CreatedById = user.Id, StatusId = status.Id };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Act
        var savedTask = await context.Tasks
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == task.Id);

        // Assert
        Assert.NotNull(savedTask);
        Assert.NotNull(savedTask.CreatedBy);
        Assert.Equal("testuser", savedTask.CreatedBy.Username);
    }

    [Fact]
    public async Task DbContext_UserShouldHaveNavigationToCreatedTasks()
    {
        // Arrange
        var context = CreateTestDbContext();
        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        context.Users.Add(user);
        context.TaskStatusDefinitions.Add(status);
        await context.SaveChangesAsync();

        var task1 = new UserTask { TaskName = "Task1", CreatedById = user.Id, StatusId = status.Id };
        var task2 = new UserTask { TaskName = "Task2", CreatedById = user.Id, StatusId = status.Id };
        context.Tasks.AddRange(task1, task2);
        await context.SaveChangesAsync();

        // Act
        var savedUser = await context.Users
            .Include(u => u.CreatedTasks)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        Assert.NotNull(savedUser);
        Assert.Equal(2, savedUser.CreatedTasks.Count);
    }

    [Fact]
    public async Task DbContext_RefreshTokenShouldBelongToUser()
    {
        // Arrange
        var context = CreateTestDbContext();
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
        var savedToken = await context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == "testtoken");

        // Assert
        Assert.NotNull(savedToken);
        Assert.NotNull(savedToken.User);
        Assert.Equal("testuser", savedToken.User.Username);
    }

    [Fact]
    public async Task DbContext_InviteShouldHaveCreatorNavigation()
    {
        // Arrange
        var context = CreateTestDbContext();
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
        var savedInvite = await context.Invites
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.InviteCode == "TESTCODE123456");

        // Assert
        Assert.NotNull(savedInvite);
        Assert.NotNull(savedInvite.CreatedBy);
        Assert.Equal("testuser", savedInvite.CreatedBy.Username);
    }
}
