using Xunit;
using BLFramework.Models;

namespace BLFramework.Tests.Models;

/// <summary>
/// Tests for the TaskStatusDefinition entity.
/// Verifies status definition properties and collection initialization.
/// </summary>
public class TaskStatusDefinitionTests
{
    [Fact]
    public void TaskStatusDefinition_ShouldInitializeWithDefaultValues()
    {
        // Act
        var status = new TaskStatusDefinition();

        // Assert
        Assert.Empty(status.StatusCode);
        Assert.Null(status.StatusDescription);
        Assert.Empty(status.ReactIcon);
        Assert.False(status.ClosingStatus);
        Assert.Empty(status.Tasks);
    }

    [Fact]
    public void TaskStatusDefinition_ShouldSetStatusCode()
    {
        // Arrange
        var statusCode = "IN_PROGRESS";
        
        // Act
        var status = new TaskStatusDefinition { StatusCode = statusCode };

        // Assert
        Assert.Equal(statusCode, status.StatusCode);
    }

    [Fact]
    public void TaskStatusDefinition_ShouldSetStatusDescription()
    {
        // Arrange
        var description = "Work is currently in progress";
        
        // Act
        var status = new TaskStatusDefinition { StatusDescription = description };

        // Assert
        Assert.Equal(description, status.StatusDescription);
    }

    [Fact]
    public void TaskStatusDefinition_ShouldSetReactIcon()
    {
        // Arrange
        var icon = "md/clock";
        
        // Act
        var status = new TaskStatusDefinition { ReactIcon = icon };

        // Assert
        Assert.Equal(icon, status.ReactIcon);
    }

    [Fact]
    public void TaskStatusDefinition_ShouldTrackClosingStatus()
    {
        // Arrange
        var status = new TaskStatusDefinition();

        // Act
        status.ClosingStatus = true;

        // Assert
        Assert.True(status.ClosingStatus);
    }

    [Fact]
    public void TaskStatusDefinition_TasksCollection_ShouldNotBeNull()
    {
        // Act
        var status = new TaskStatusDefinition();

        // Assert
        Assert.NotNull(status.Tasks);
    }
}
