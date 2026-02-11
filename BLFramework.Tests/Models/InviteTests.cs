using Xunit;
using BLFramework.Models;

namespace BLFramework.Tests.Models;

/// <summary>
/// Tests for the Invite entity.
/// Verifies invite status properties, code generation, and expiration logic.
/// </summary>
public class InviteTests
{
    [Fact]
    public void Invite_ShouldInitializeWithEmptyCode()
    {
        // Act
        var invite = new Invite();

        // Assert
        Assert.Empty(invite.InviteCode);
        Assert.Equal(0, invite.CreatedById);
        Assert.Null(invite.UsedOn);
        Assert.Null(invite.UsedById);
    }

    [Fact]
    public void Invite_IsValid_ShouldReturnTrueWhenNotExpiredAndNotUsed()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);
        var invite = new Invite
        {
            ExpiresOn = futureDate,
            UsedOn = null
        };

        // Act
        var isValid = invite.IsValid;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Invite_IsValid_ShouldReturnFalseWhenExpired()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var invite = new Invite
        {
            ExpiresOn = pastDate,
            UsedOn = null
        };

        // Act
        var isValid = invite.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Invite_IsValid_ShouldReturnFalseWhenUsed()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);
        var invite = new Invite
        {
            ExpiresOn = futureDate,
            UsedOn = DateTime.UtcNow
        };

        // Act
        var isValid = invite.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Invite_IsExpired_ShouldReturnTrueWhenExpirationPassed()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var invite = new Invite { ExpiresOn = pastDate };

        // Act
        var isExpired = invite.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void Invite_IsExpired_ShouldReturnFalseWhenNotExpired()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);
        var invite = new Invite { ExpiresOn = futureDate };

        // Act
        var isExpired = invite.IsExpired;

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void Invite_IsUsed_ShouldReturnTrueWhenUsedOnIsSet()
    {
        // Arrange
        var invite = new Invite { UsedOn = DateTime.UtcNow };

        // Act
        var isUsed = invite.IsUsed;

        // Assert
        Assert.True(isUsed);
    }

    [Fact]
    public void Invite_IsUsed_ShouldReturnFalseWhenUsedOnIsNull()
    {
        // Arrange
        var invite = new Invite { UsedOn = null };

        // Act
        var isUsed = invite.IsUsed;

        // Assert
        Assert.False(isUsed);
    }

    [Fact]
    public void Invite_GenerateInviteCode_ShouldReturn16Characters()
    {
        // Act
        var code = Invite.GenerateInviteCode();

        // Assert
        Assert.Equal(16, code.Length);
    }

    [Fact]
    public void Invite_GenerateInviteCode_ShouldReturnOnlyUppercaseAlphanumeric()
    {
        // Act
        var code = Invite.GenerateInviteCode();

        // Assert
        Assert.Matches(@"^[A-Z0-9]{16}$", code);
    }

    [Fact]
    public void Invite_GenerateInviteCode_ShouldGenerateUniqueCodes()
    {
        // Act
        var code1 = Invite.GenerateInviteCode();
        var code2 = Invite.GenerateInviteCode();
        var code3 = Invite.GenerateInviteCode();

        // Assert
        Assert.NotEqual(code1, code2);
        Assert.NotEqual(code2, code3);
        Assert.NotEqual(code1, code3);
    }

    [Fact]
    public void Invite_CreatedBy_ShouldSetAndGetNavigationProperty()
    {
        // Arrange
        var invite = new Invite();
        var creator = new User
        {
            Id = 1,
            Username = "creator",
            Password = "hashed",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        // Act
        invite.CreatedBy = creator;

        // Assert
        Assert.NotNull(invite.CreatedBy);
        Assert.Equal(1, invite.CreatedBy.Id);
        Assert.Equal("creator", invite.CreatedBy.Username);
    }

    [Fact]
    public void Invite_UsedBy_ShouldSetAndGetNavigationProperty()
    {
        // Arrange
        var invite = new Invite();
        var user = new User
        {
            Id = 2,
            Username = "user",
            Password = "hashed",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        // Act
        invite.UsedBy = user;

        // Assert
        Assert.NotNull(invite.UsedBy);
        Assert.Equal(2, invite.UsedBy.Id);
        Assert.Equal("user", invite.UsedBy.Username);
    }

    [Fact]
    public void Invite_CreatedBy_ShouldAllowNull()
    {
        // Arrange
        var invite = new Invite
        {
            CreatedBy = new User { Id = 1, Username = "test", Password = "hash", IsActive = true, CreatedAt = DateTime.UtcNow, LastLogin = DateTime.UtcNow }
        };

        // Act
        invite.CreatedBy = null;

        // Assert
        Assert.Null(invite.CreatedBy);
    }

    [Fact]
    public void Invite_UsedBy_ShouldAllowNull()
    {
        // Arrange
        var invite = new Invite
        {
            UsedBy = new User { Id = 1, Username = "test", Password = "hash", IsActive = true, CreatedAt = DateTime.UtcNow, LastLogin = DateTime.UtcNow }
        };

        // Act
        invite.UsedBy = null;

        // Assert
        Assert.Null(invite.UsedBy);
    }
}
