using Xunit;
using Microsoft.EntityFrameworkCore;
using BLFramework.Data;
using BLFramework.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace BLFramework.Tests.Services;

public class SimpleClosedOnTest
{
    private AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task DirectModificationWorks()
    {
        var context = CreateTestDbContext();
        var status = new TaskStatusDefinition { StatusCode = "TODO", ClosingStatus = false };
        var user = new User { Username = "testuser", Password = "hash", IsActive = true };
        context.TaskStatusDefinitions.Add(status);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var task = new UserTask { TaskName = "Task", CreatedById = user.Id, StatusId = status.Id, StatusPriority = 0 };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Direct modification
        task.ClosedOn = DateTime.UtcNow;
        task.ModifiedOn = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Verify
        var dbTask = await context.Tasks.FindAsync(task.Id);
        Assert.NotNull(dbTask?.ClosedOn);
    }
}
