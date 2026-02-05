using BLFramework.Data;
using BLFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TaskFlowAPI.Services
{
    /// <summary>
    /// Configuration for closed task cleanup
    /// </summary>
    public class ClosedTaskCleanupOptions
    {
        public bool Enabled { get; set; } = true;
        public int IntervalDays { get; set; } = 7; // Run cleanup every 7 days
        public int DeleteAfterDays { get; set; } = 30; // Delete tasks closed more than 30 days ago
        public int BatchSize { get; set; } = 100; // Number of tasks to delete in each batch
    }

    /// <summary>
    /// Background service that periodically deletes old closed tasks
    /// </summary>
    public class ClosedTaskCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClosedTaskCleanupService> _logger;
        private readonly ClosedTaskCleanupOptions _options;
        private Timer? _timer;

        public ClosedTaskCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ClosedTaskCleanupService> logger,
            IOptions<ClosedTaskCleanupOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Closed task cleanup service is disabled");
                return;
            }

            _logger.LogInformation("Closed task cleanup service started. Will run every {IntervalDays} days and delete tasks closed more than {DeleteAfterDays} days ago",
                _options.IntervalDays, _options.DeleteAfterDays);

            // Run immediately on startup after a small delay
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            await CleanupOldClosedTasksAsync(stoppingToken);

            // Set up recurring cleanup
            _timer = new Timer(
                async (_) => await CleanupOldClosedTasksAsync(stoppingToken),
                null,
                TimeSpan.FromDays(_options.IntervalDays),
                TimeSpan.FromDays(_options.IntervalDays));
        }

        private async System.Threading.Tasks.Task CleanupOldClosedTasksAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();

                    // Calculate the cutoff date
                    var cutoffDate = DateTime.UtcNow.AddDays(-_options.DeleteAfterDays);

                    _logger.LogInformation("Starting cleanup of closed tasks older than {CutoffDate}", cutoffDate);

                    // Find and delete old closed tasks in batches
                    int totalDeleted = 0;
                    bool hasMoreTasks = true;

                    while (hasMoreTasks && !cancellationToken.IsCancellationRequested)
                    {
                        // Get a batch of old closed tasks
                        var oldClosedTasks = await context.Tasks
                            .Where(t => t.ClosedOn != null && t.ClosedOn < cutoffDate)
                            .OrderBy(t => t.ClosedOn)
                            .Take(_options.BatchSize)
                            .ToListAsync(cancellationToken);

                        if (oldClosedTasks.Count == 0)
                        {
                            hasMoreTasks = false;
                            break;
                        }

                        // Delete each task
                        foreach (var task in oldClosedTasks)
                        {
                            try
                            {
                                await taskService.DeleteAsync(task);
                                totalDeleted++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error deleting closed task {TaskId}", task.Id);
                            }
                        }

                        _logger.LogInformation("Deleted {Count} old closed tasks in this batch", oldClosedTasks.Count);
                    }

                    _logger.LogInformation("Cleanup completed. Total tasks deleted: {TotalDeleted}", totalDeleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of old closed tasks");
            }
        }

        public override async System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            _logger.LogInformation("Closed task cleanup service stopped");
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
