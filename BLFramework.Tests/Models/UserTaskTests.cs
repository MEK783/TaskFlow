using Xunit;
using BLFramework.Models;

namespace BLFramework.Tests.Models;

/// <summary>
/// Tests for the UserTask entity.
/// Verifies task properties, status updates, and closing behavior.
/// </summary>
public class UserTaskTests
{
    [Fact]
    public void UserTask_ShouldInitializeWithDefaultValues()
    {
        // Act
        var task = new UserTask();

        // Assert
        Assert.Empty(task.TaskName);
        Assert.Null(task.TaskDescription);
        Assert.Equal(0, task.StatusId);
        Assert.Equal(0, task.CreatedById);
        Assert.Equal(0, task.StatusPriority);
        Assert.NotEqual(DateTime.MinValue, task.ModifiedOn);
        Assert.Null(task.ClosedOn);
    }

    [Fact]
    public void UserTask_ShouldSetTaskName()
    {
        // Arrange
        var taskName = "Complete project proposal";
        
        // Act
        var task = new UserTask { TaskName = taskName };

        // Assert
        Assert.Equal(taskName, task.TaskName);
    }

    [Fact]
    public void UserTask_ShouldSetTaskDescription()
    {
        // Arrange
        var description = "Review and submit project proposal by Friday";
        
        // Act
        var task = new UserTask { TaskDescription = description };

        // Assert
        Assert.Equal(description, task.TaskDescription);
    }

    [Fact]
    public void UserTask_ShouldSetStatusId()
    {
        // Arrange
        var statusId = 2;
        
        // Act
        var task = new UserTask { StatusId = statusId };

        // Assert
        Assert.Equal(statusId, task.StatusId);
    }

    [Fact]
    public void UserTask_ShouldSetStatusPriority()
    {
        // Arrange
        var priority = 3;
        
        // Act
        var task = new UserTask { StatusPriority = priority };

        // Assert
        Assert.Equal(priority, task.StatusPriority);
    }

    [Fact]
    public void UserTask_UpdateClosedStatusBasedOnStatusDefinition_ShouldSetClosedOnWhenStatusIsClosing()
    {
        // Arrange
        var task = new UserTask { ClosedOn = null };
        var status = new TaskStatusDefinition { ClosingStatus = true };
        task.Status = status;

        // Act
        task.UpdateClosedStatusBasedOnStatusDefinition();

        // Assert
        Assert.NotNull(task.ClosedOn);
    }

    [Fact]
    public void UserTask_UpdateClosedStatusBasedOnStatusDefinition_ShouldClearClosedOnWhenStatusIsNotClosing()
    {
        // Arrange
        var closedTime = DateTime.UtcNow.AddDays(-1);
        var task = new UserTask { ClosedOn = closedTime };
        var status = new TaskStatusDefinition { ClosingStatus = false };
        task.Status = status;

        // Act
        task.UpdateClosedStatusBasedOnStatusDefinition();

        // Assert
        Assert.Null(task.ClosedOn);
    }

    [Fact]
    public void UserTask_UpdateClosedStatusBasedOnStatusDefinition_ShouldReturnEarlyWhenStatusIsNull()
    {
        // Arrange
        var task = new UserTask { Status = null };

        // Act & Assert (should not throw)
        task.UpdateClosedStatusBasedOnStatusDefinition();
        Assert.Null(task.ClosedOn);
    }

    [Fact]
    public void UserTask_ShouldNotSetClosedOnTwice()
    {
        // Arrange
        var firstClosedTime = DateTime.UtcNow.AddDays(-2);
        var task = new UserTask { ClosedOn = firstClosedTime };
        var status = new TaskStatusDefinition { ClosingStatus = true };
        task.Status = status;

        // Act
        task.UpdateClosedStatusBasedOnStatusDefinition();

        // Assert - ClosedOn should remain the same (not updated again)
        Assert.Equal(firstClosedTime, task.ClosedOn);
    }
}
