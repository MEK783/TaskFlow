using Xunit;
using BLFramework.Models;

namespace BLFramework.Tests.Models;

/// <summary>
/// Tests for the User entity.
/// Verifies user property initialization, default values, and collection initialization.
/// </summary>
public class UserTests
{
    [Fact]
    public void User_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        Assert.Empty(user.Username);
        Assert.Empty(user.Password);
        Assert.True(user.IsActive);
        Assert.NotEqual(DateTime.MinValue, user.LastLogin);
        Assert.Empty(user.CreatedTasks);
        Assert.Empty(user.CreatedInvites);
        Assert.Empty(user.UsedInvites);
        Assert.Empty(user.RefreshTokens);
    }

    [Fact]
    public void User_ShouldSetUsernameAndPassword()
    {
        // Arrange
        var username = "testuser";
        var password = "hashedpassword123";

        // Act
        var user = new User 
        { 
            Username = username, 
            Password = password 
        };

        // Assert
        Assert.Equal(username, user.Username);
        Assert.Equal(password, user.Password);
    }

    [Fact]
    public void User_ShouldAllowDeactivation()
    {
        // Arrange
        var user = new User { IsActive = true };

        // Act
        user.IsActive = false;

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void User_ShouldTrackLastLogin()
    {
        // Arrange
        var user = new User();
        var newLoginTime = DateTime.UtcNow.AddHours(-1);

        // Act
        user.LastLogin = newLoginTime;

        // Assert
        Assert.Equal(newLoginTime, user.LastLogin);
    }

    [Fact]
    public void User_NavigationCollections_ShouldNotBeNull()
    {
        // Act
        var user = new User();

        // Assert
        Assert.NotNull(user.CreatedTasks);
        Assert.NotNull(user.CreatedInvites);
        Assert.NotNull(user.UsedInvites);
        Assert.NotNull(user.RefreshTokens);
    }
}
