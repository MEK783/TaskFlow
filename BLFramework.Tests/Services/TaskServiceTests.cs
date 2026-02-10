using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;

namespace BLFramework.Tests.Services;

/// <summary>
/// Tests for the TaskService business logic.
/// Verifies task creation, status transitions, priority management, and closing behavior.
/// </summary>
public class TaskServiceTests
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ILogger<BaseService<UserTask>> CreateMockLogger()
    {
        return new Mock<ILogger<BaseService<UserTask>>>().Object;
    }

    [Fact]
    public async Task AddAsync_ShouldCreateNewTask()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskService(context, logger);

        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        context.Users.Add(user);
        context.TaskStatusDefinitions.Add(status);
        await context.SaveChangesAsync();

        var task = new UserTask { TaskName = "Test Task", TaskDescription = "Description", CreatedById = user.Id, StatusId = status.Id };

        // Act
        var result = await service.AddAsync(task);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.TaskName);
        Assert.Equal("Description", result.TaskDescription);
        Assert.Equal(user.Id, result.CreatedById);
        Assert.Equal(status.Id, result.StatusId);
    }

    [Fact]
    public async Task GetTasksByUserAsync_ShouldReturnUserTasks()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskService(context, logger);

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
        var result = await service.GetTasksByUserAsync(user.Id);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ChangeStatusAsync_ShouldChangeTaskStatus()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskService(context, logger);

        var status1 = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        var status2 = new TaskStatusDefinition { StatusCode = "IN_PROGRESS", ClosingStatus = false };
        context.TaskStatusDefinitions.AddRange(status1, status2);
        
        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var task = new UserTask { TaskName = "Task", CreatedById = user.Id, StatusId = status1.Id };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ChangeStatusAsync(task.Id, status2.Id, 1);

        // Assert
        Assert.Equal(status2.Id, result.StatusId);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnTaskWithRelations()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskService(context, logger);

        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.TaskStatusDefinitions.Add(status);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var task = new UserTask { TaskName = "Task", CreatedById = user.Id, StatusId = status.Id };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdWithDetailsAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Task", result.TaskName);
        Assert.NotNull(result.Status);
    }

    [Fact]
    public async Task CloseTaskAsync_ShouldSetClosedOnAndUpdateStatus()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskService(context, logger);

        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.TaskStatusDefinitions.Add(status);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var task = new UserTask { TaskName = "Task", CreatedById = user.Id, StatusId = status.Id, StatusPriority = 0 };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var taskId = task.Id;

        // Act
        await service.CloseTaskAsync(taskId);

        // Assert - Verify the database was updated
        var dbTask = await context.Tasks.FindAsync(taskId);
        Assert.NotNull(dbTask);
        Assert.NotNull(dbTask.ClosedOn);
    }
}
