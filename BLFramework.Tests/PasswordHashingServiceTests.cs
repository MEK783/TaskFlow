using Xunit;
using BLFramework.Services;

namespace BLFramework.Tests;

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
}
