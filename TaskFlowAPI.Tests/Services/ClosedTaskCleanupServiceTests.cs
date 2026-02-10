using System.Reflection;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskFlowAPI.Services;
using Xunit;

namespace TaskFlowAPI.Tests.Services
{
    public class ClosedTaskCleanupServiceTests
    {
        [Fact]
        public void Options_Defaults_AreSet()
        {
            var options = new ClosedTaskCleanupOptions();

            Assert.Equal(7, options.IntervalDays);
            Assert.Equal(10, options.StartupDelaySeconds);
        }

        [Fact(Timeout = 30000)]
        public async Task CleanupOldClosedTasksAsync_DeletesOnlyEligibleTasks()
        {
            var databaseName = $"ClosedTaskCleanupTests-{Guid.NewGuid()}";
            var services = BuildServiceProvider(databaseName);
            await SeedTasksAsync(services, DateTime.UtcNow);

            using (var seedScope = services.CreateScope())
            {
                var seedContext = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var seededCount = await seedContext.Tasks.CountAsync();
                Assert.Equal(3, seededCount);
            }

            var cleanupService = CreateCleanupService(services, new ClosedTaskCleanupOptions
            {
                Enabled = true,
                DeleteAfterDays = 30,
                IntervalDays = 7,
                BatchSize = 100
            });

            await InvokeCleanupAsync(cleanupService, CancellationToken.None);

            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var remainingTasks = await context.Tasks.ToListAsync();

            Assert.DoesNotContain(remainingTasks, t => t.TaskName == "Old Closed");
            Assert.Contains(remainingTasks, t => t.TaskName == "Recent Closed");
            Assert.Contains(remainingTasks, t => t.TaskName == "Open Task");
        }

        [Fact(Timeout = 30000)]
        public async Task ExecuteAsync_WhenEnabled_ConfiguresTimer_AndStopsCleanly()
        {
            var databaseName = $"ClosedTaskCleanupTests-{Guid.NewGuid()}";
            var services = BuildServiceProvider(databaseName);

            var cleanupService = CreateCleanupService(services, new ClosedTaskCleanupOptions
            {
                Enabled = true,
                DeleteAfterDays = 30,
                IntervalDays = 1,
                BatchSize = 100,
                StartupDelaySeconds = 0
            });

            await InvokeExecuteAsync(cleanupService, CancellationToken.None);

            await cleanupService.StopAsync(CancellationToken.None);
            cleanupService.Dispose();
        }

        [Fact(Timeout = 30000)]
        public async Task ExecuteAsync_WhenDisabled_CompletesWithoutError()
        {
            var databaseName = $"ClosedTaskCleanupTests-{Guid.NewGuid()}";
            var services = BuildServiceProvider(databaseName);
            var cleanupService = CreateCleanupService(services, new ClosedTaskCleanupOptions
            {
                Enabled = false,
                DeleteAfterDays = 30,
                IntervalDays = 7,
                BatchSize = 100,
                StartupDelaySeconds = 0
            });

            var executeTask = InvokeExecuteAsync(cleanupService, CancellationToken.None);
            await executeTask;

            await cleanupService.StopAsync(CancellationToken.None);
            cleanupService.Dispose();
        }

        [Fact(Timeout = 30000)]
        public async Task CleanupOldClosedTasksAsync_HandlesDeleteErrors()
        {
            var databaseName = $"ClosedTaskCleanupTests-{Guid.NewGuid()}";
            var services = BuildServiceProvider(databaseName, useThrowingTaskService: true);
            await SeedTasksAsync(services, DateTime.UtcNow);

            var cleanupService = CreateCleanupService(services, new ClosedTaskCleanupOptions
            {
                Enabled = true,
                DeleteAfterDays = 30,
                IntervalDays = 7,
                BatchSize = 100,
                StartupDelaySeconds = 0
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            await InvokeCleanupAsync(cleanupService, cts.Token);

            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var remainingTasks = await context.Tasks.ToListAsync();

            Assert.Contains(remainingTasks, t => t.TaskName == "Old Closed");
        }

        [Fact(Timeout = 30000)]
        public async Task CleanupOldClosedTasksAsync_HandlesScopeFailures()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            var logger = loggerFactory.CreateLogger<ClosedTaskCleanupService>();
            var options = Options.Create(new ClosedTaskCleanupOptions
            {
                Enabled = true,
                StartupDelaySeconds = 0
            });

            var cleanupService = new ClosedTaskCleanupService(new ThrowingServiceProvider(), logger, options);

            await InvokeCleanupAsync(cleanupService, CancellationToken.None);
        }

        private static ServiceProvider BuildServiceProvider(string databaseName, bool useThrowingTaskService = false)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
            if (useThrowingTaskService)
            {
                services.AddScoped<TaskService, ThrowingTaskService>();
            }
            else
            {
                services.AddScoped<TaskService>();
            }
            services.AddScoped(typeof(ILogger<BaseService<UserTask>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<UserTask>>());

            return services.BuildServiceProvider();
        }

        private static ClosedTaskCleanupService CreateCleanupService(
            IServiceProvider services,
            ClosedTaskCleanupOptions options)
        {
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<ClosedTaskCleanupService>();
            return new ClosedTaskCleanupService(services, logger, Options.Create(options));
        }

        private static async Task SeedTasksAsync(ServiceProvider services, DateTime now)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            var user = new User
            {
                Id = 1,
                Username = "cleanup-user",
                Password = "hashed",
                IsActive = true,
                CreatedAt = now.AddDays(-10),
                LastLogin = now.AddDays(-1)
            };

            var status = new TaskStatusDefinition
            {
                Id = 1,
                StatusCode = "DONE",
                StatusDescription = "Done",
                ReactIcon = "ai/checkCircle",
                ClosingStatus = true
            };

            context.Users.Add(user);
            context.TaskStatusDefinitions.Add(status);

            context.Tasks.AddRange(
                new UserTask
                {
                    Id = 100,
                    TaskName = "Old Closed",
                    StatusId = status.Id,
                    CreatedById = user.Id,
                    StatusPriority = 0,
                    CreatedAt = now.AddDays(-60),
                    ModifiedOn = now.AddDays(-45),
                    ClosedOn = now.AddDays(-40)
                },
                new UserTask
                {
                    Id = 101,
                    TaskName = "Recent Closed",
                    StatusId = status.Id,
                    CreatedById = user.Id,
                    StatusPriority = 1,
                    CreatedAt = now.AddDays(-20),
                    ModifiedOn = now.AddDays(-10),
                    ClosedOn = now.AddDays(-5)
                },
                new UserTask
                {
                    Id = 102,
                    TaskName = "Open Task",
                    StatusId = status.Id,
                    CreatedById = user.Id,
                    StatusPriority = 2,
                    CreatedAt = now.AddDays(-5),
                    ModifiedOn = now.AddDays(-1),
                    ClosedOn = null
                });

            await context.SaveChangesAsync();
        }

        private static async Task InvokeCleanupAsync(ClosedTaskCleanupService service, CancellationToken token)
        {
            var method = typeof(ClosedTaskCleanupService)
                .GetMethod("CleanupOldClosedTasksAsync", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(method);
            var task = (Task)method!.Invoke(service, new object[] { token })!;
            await task;
        }

        private static async Task InvokeExecuteAsync(ClosedTaskCleanupService service, CancellationToken token)
        {
            var method = typeof(ClosedTaskCleanupService)
                .GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(method);
            var task = (Task)method!.Invoke(service, new object[] { token })!;
            await task;
        }

        private sealed class ThrowingTaskService : TaskService
        {
            public ThrowingTaskService(AppDbContext context, ILogger<BaseService<UserTask>> logger)
                : base(context, logger)
            {
            }

            public override async Task DeleteAsync(UserTask entity)
            {
                await Task.Yield();
                throw new InvalidOperationException("Delete failed");
            }
        }

        private sealed class ThrowingServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType)
            {
                throw new InvalidOperationException("Scope creation failed");
            }
        }
    }
}
