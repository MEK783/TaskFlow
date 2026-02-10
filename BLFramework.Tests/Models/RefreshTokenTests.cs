using Xunit;
using BLFramework.Models;

namespace BLFramework.Tests.Models;

/// <summary>
/// Tests for the RefreshToken entity.
/// Verifies token status properties (IsActive, IsExpired, IsRevoked) and their interactions.
/// </summary>
public class RefreshTokenTests
{
    [Fact]
    public void RefreshToken_ShouldInitializeWithEmptyToken()
    {
        // Act
        var token = new RefreshToken();

        // Assert
        Assert.Empty(token.Token);
    }

    [Fact]
    public void RefreshToken_IsActive_ShouldReturnTrueWhenNotExpiredAndNotRevoked()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);
        var token = new RefreshToken
        {
            ExpiresOn = futureDate,
            RevokedOn = null
        };

        // Act
        var isActive = token.IsActive;

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void RefreshToken_IsActive_ShouldReturnFalseWhenExpired()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var token = new RefreshToken
        {
            ExpiresOn = pastDate,
            RevokedOn = null
        };

        // Act
        var isActive = token.IsActive;

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void RefreshToken_IsActive_ShouldReturnFalseWhenRevoked()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);
        var token = new RefreshToken
        {
            ExpiresOn = futureDate,
            RevokedOn = DateTime.UtcNow
        };

        // Act
        var isActive = token.IsActive;

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void RefreshToken_IsExpired_ShouldReturnTrueWhenExpirationPassed()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var token = new RefreshToken { ExpiresOn = pastDate };

        // Act
        var isExpired = token.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void RefreshToken_IsExpired_ShouldReturnFalseWhenNotExpired()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);
        var token = new RefreshToken { ExpiresOn = futureDate };

        // Act
        var isExpired = token.IsExpired;

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void RefreshToken_IsRevoked_ShouldReturnTrueWhenRevokedOnIsSet()
    {
        // Arrange
        var token = new RefreshToken { RevokedOn = DateTime.UtcNow };

        // Act
        var isRevoked = token.IsRevoked;

        // Assert
        Assert.True(isRevoked);
    }

    [Fact]
    public void RefreshToken_IsRevoked_ShouldReturnFalseWhenRevokedOnIsNull()
    {
        // Arrange
        var token = new RefreshToken { RevokedOn = null };

        // Act
        var isRevoked = token.IsRevoked;

        // Assert
        Assert.False(isRevoked);
    }

    [Fact]
    public void RefreshToken_ShouldAllowSettingReplacingToken()
    {
        // Arrange
        var token = new RefreshToken();
        var newToken = "NewTokenString";

        // Act
        token.ReplacingToken = newToken;

        // Assert
        Assert.Equal(newToken, token.ReplacingToken);
    }
}
