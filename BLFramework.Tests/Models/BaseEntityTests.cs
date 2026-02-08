using Xunit;
using BLFramework.Models;

namespace BLFramework.Tests.Models;

/// <summary>
/// Tests for the BaseEntity abstract class.
/// Verifies audit timestamp initialization and property assignment.
/// </summary>
public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var entity = new User();

        // Assert
        Assert.Equal(0, entity.Id);
        Assert.NotEqual(DateTime.MinValue, entity.CreatedAt);
        Assert.Null(entity.UpdatedAt);
    }

    [Fact]
    public void BaseEntity_ShouldSetIdCorrectly()
    {
        // Arrange
        var testId = 42;
        
        // Act
        var entity = new User { Id = testId };

        // Assert
        Assert.Equal(testId, entity.Id);
    }

    [Fact]
    public void BaseEntity_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var entity = new User();
        
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(entity.CreatedAt >= beforeCreation && entity.CreatedAt <= afterCreation);
    }

    [Fact]
    public void BaseEntity_ShouldAllowUpdatingUpdatedAt()
    {
        // Arrange
        var entity = new User();
        var updateTime = DateTime.UtcNow;

        // Act
        entity.UpdatedAt = updateTime;

        // Assert
        Assert.NotNull(entity.UpdatedAt);
        Assert.Equal(updateTime, entity.UpdatedAt);
    }
}
