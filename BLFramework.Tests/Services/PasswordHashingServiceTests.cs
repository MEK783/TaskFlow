using Xunit;
using BLFramework.Services;

namespace BLFramework.Tests.Services;

/// <summary>
/// Tests for the PasswordHashingService static service.
/// Verifies Argon2id hashing and verification functionality.
/// </summary>
public class PasswordHashingServiceTests
{
    [Fact]
    public void HashPassword_ShouldNotReturnNull()
    {
        // Arrange
        var sha512Hash = "TestPassword123!";

        // Act
        var hashedPassword = PasswordHashingService.HashPassword(sha512Hash);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashForSamePassword()
    {
        // Arrange
        var sha512Hash = "TestPassword123!";

        // Act
        var hash1 = PasswordHashingService.HashPassword(sha512Hash);
        var hash2 = PasswordHashingService.HashPassword(sha512Hash);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var sha512Hash = "TestPassword123!";
        var hashedPassword = PasswordHashingService.HashPassword(sha512Hash);

        // Act
        var result = PasswordHashingService.VerifyPassword(sha512Hash, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var sha512Hash = "TestPassword123!";
        var incorrectSha512Hash = "WrongPassword123!";
        var hashedPassword = PasswordHashingService.HashPassword(sha512Hash);

        // Act
        var result = PasswordHashingService.VerifyPassword(incorrectSha512Hash, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForNullHash()
    {
        // Arrange
        var sha512Hash = "TestPassword123!";
        var hashedPassword = PasswordHashingService.HashPassword(sha512Hash);

        // Act
        var result = PasswordHashingService.VerifyPassword(sha512Hash, "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_ShouldHandleLongPasswords()
    {
        // Arrange
        var longPassword = new string('a', 500);

        // Act
        var hashedPassword = PasswordHashingService.HashPassword(longPassword);

        // Assert
        Assert.NotEmpty(hashedPassword);
        Assert.True(PasswordHashingService.VerifyPassword(longPassword, hashedPassword));
    }
}
