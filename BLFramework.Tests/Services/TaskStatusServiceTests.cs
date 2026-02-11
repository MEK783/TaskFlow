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

    [Fact]
    public async Task GetByStatusCodeAsync_ShouldReturnStatusWhenFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        var status = new TaskStatusDefinition { StatusCode = "IN_PROGRESS", ClosingStatus = false };
        context.TaskStatusDefinitions.Add(status);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByStatusCodeAsync("IN_PROGRESS");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("IN_PROGRESS", result.StatusCode);
    }

    [Fact]
    public async Task GetByStatusCodeAsync_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act
        var result = await service.GetByStatusCodeAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByStatusCodeAsync_ShouldThrowWhenStatusCodeIsNull()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByStatusCodeAsync(null!));
    }

    [Fact]
    public async Task GetByStatusCodeAsync_ShouldThrowWhenStatusCodeIsEmpty()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByStatusCodeAsync(string.Empty));
    }

    [Fact]
    public async Task GetByStatusCodeAsync_ShouldThrowWhenStatusCodeIsWhitespace()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByStatusCodeAsync("   "));
    }

    [Fact]
    public async Task GetOpenStatusesAsync_ShouldReturnOnlyOpenStatuses()
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
        var result = await service.GetOpenStatusesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.False(s.ClosingStatus));
    }

    [Fact]
    public async Task GetOpenStatusesAsync_ShouldReturnEmptyListWhenNoOpenStatuses()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        var status = new TaskStatusDefinition { StatusCode = "DONE", ClosingStatus = true };
        context.TaskStatusDefinitions.Add(status);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetOpenStatusesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task StatusCodeExistsAsync_ShouldReturnTrueWhenExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        context.TaskStatusDefinitions.Add(status);
        await context.SaveChangesAsync();

        // Act
        var result = await service.StatusCodeExistsAsync("TODO");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task StatusCodeExistsAsync_ShouldReturnFalseWhenNotExists()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act
        var result = await service.StatusCodeExistsAsync("NONEXISTENT");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task StatusCodeExistsAsync_ShouldReturnFalseWhenStatusCodeIsNull()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act
        var result = await service.StatusCodeExistsAsync(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task StatusCodeExistsAsync_ShouldReturnFalseWhenStatusCodeIsEmpty()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act
        var result = await service.StatusCodeExistsAsync(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task StatusCodeExistsAsync_ShouldReturnFalseWhenStatusCodeIsWhitespace()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Act
        var result = await service.StatusCodeExistsAsync("   ");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);
        var status = new TaskStatusDefinition { StatusCode = "NEW", ClosingStatus = false };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => service.AddAsync(status));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);
        var status = new TaskStatusDefinition { Id = 1, StatusCode = "TODO", ClosingStatus = false };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => service.UpdateAsync(status));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);
        var status = new TaskStatusDefinition { Id = 1, StatusCode = "TODO", ClosingStatus = false };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => service.DeleteAsync(status));
    }

    [Fact]
    public async Task GetByStatusCodeAsync_ShouldThrowAndLogWhenDatabaseErrorOccurs()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Dispose context to trigger exception
        await context.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => service.GetByStatusCodeAsync("TODO"));
    }

    [Fact]
    public async Task GetClosingStatusesAsync_ShouldThrowAndLogWhenDatabaseErrorOccurs()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Dispose context to trigger exception
        await context.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => service.GetClosingStatusesAsync());
    }

    [Fact]
    public async Task GetOpenStatusesAsync_ShouldThrowAndLogWhenDatabaseErrorOccurs()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Dispose context to trigger exception
        await context.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => service.GetOpenStatusesAsync());
    }

    [Fact]
    public async Task StatusCodeExistsAsync_ShouldThrowAndLogWhenDatabaseErrorOccurs()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        // Dispose context to trigger exception
        await context.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => service.StatusCodeExistsAsync("TODO"));
    }

    [Fact]
    public async Task GetClosingStatusesAsync_ShouldReturnEmptyListWhenNoClosingStatuses()
    {
        // Arrange
        var context = CreateTestDbContext();
        var logger = CreateMockLogger();
        var service = new TaskStatusService(context, logger);

        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        context.TaskStatusDefinitions.Add(status);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetClosingStatusesAsync();

        // Assert
        Assert.Empty(result);
    }
}
