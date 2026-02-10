using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BLFramework.Configuration;
using BLFramework.Data;
using BLFramework.Services;

namespace BLFramework.Tests.Configuration;

/// <summary>
/// Tests for the DbContextExtensions configuration helper.
/// Verifies dependency injection setup and service registration.
/// </summary>
public class DbContextExtensionsTests
{
    private class NullLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    [Fact]
    public void AddAppDbContext_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConnectionString = "Data Source=:memory:;";

        // Act
        services.AddAppDbContext(inMemoryConnectionString);
        var provider = services.BuildServiceProvider();

        // Assert
        var dbContext = provider.GetService<AppDbContext>();
        Assert.NotNull(dbContext);
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterUserService()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConnectionString = "Data Source=:memory:;";

        // Act
        services.AddAppDbContext(inMemoryConnectionString);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<UserService>();
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<UserService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterTaskService()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConnectionString = "Data Source=:memory:;";

        // Act
        services.AddAppDbContext(inMemoryConnectionString);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<TaskService>();
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<TaskService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterRefreshTokenService()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConnectionString = "Data Source=:memory:;";

        // Act
        services.AddAppDbContext(inMemoryConnectionString);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<RefreshTokenService>();
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<RefreshTokenService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterTaskStatusService()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConnectionString = "Data Source=:memory:;";

        // Act
        services.AddAppDbContext(inMemoryConnectionString);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<TaskStatusService>();
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<TaskStatusService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterInviteService()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConnectionString = "Data Source=:memory:;";

        // Act
        services.AddAppDbContext(inMemoryConnectionString);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<InviteService>();
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<InviteService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterAuthenticationService()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConnectionString = "Data Source=:memory:;";

        // Act
        services.AddAppDbContext(inMemoryConnectionString);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<UserService>();
        services.AddScoped<InviteService>();
        services.AddScoped<AuthenticationService>();
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<AuthenticationService>();
        Assert.NotNull(service);
    }
}
