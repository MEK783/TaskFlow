using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;

namespace BLFramework.Tests.Services;

/// <summary>
/// Tests for the TaskStatusService business logic.
/// Verifies status definition retrieval and filtering operations.
/// </summary>
public class TaskStatusServiceTests
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ILogger<TaskStatusService> CreateMockLogger()
    {
        return new Mock<ILogger<TaskStatusService>>().Object;
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnStatusWhenFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        context.TaskStatusDefinitions.Add(status);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(status.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TODO", result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDefinitions()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        var status1 = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        var status2 = new TaskStatusDefinition { StatusCode = "IN_PROGRESS", ClosingStatus = false };
        var status3 = new TaskStatusDefinition { StatusCode = "DONE", ClosingStatus = true };
        context.TaskStatusDefinitions.AddRange(status1, status2, status3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetClosingStatusesAsync_ShouldReturnOnlyClosingStatuses()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        var status1 = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        var status2 = new TaskStatusDefinition { StatusCode = "DONE", ClosingStatus = true };
        var status3 = new TaskStatusDefinition { StatusCode = "CLOSED", ClosingStatus = true };
        context.TaskStatusDefinitions.AddRange(status1, status2, status3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetClosingStatusesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.True(s.ClosingStatus));
    }
}
