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
    /// Configuration options for the closed task cleanup service.
    /// Controls whether cleanup is enabled, how often it runs, and which tasks are eligible for deletion.
    /// </summary>
    public class ClosedTaskCleanupOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the cleanup service is enabled.
        /// Defaults to true. Set to false to disable automatic cleanup.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the interval in days between cleanup runs.
        /// Defaults to 7 days. The service will execute cleanup this frequently.
        /// </summary>
        public int IntervalDays { get; set; } = 7;

        /// <summary>
        /// Gets or sets the startup delay in seconds before the first cleanup run.
        /// Defaults to 10 seconds to allow the application to finish startup.
        /// </summary>
        public int StartupDelaySeconds { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of days after closure before a task is eligible for deletion.
        /// Defaults to 30 days. Only tasks closed more than this many days ago will be deleted.
        /// </summary>
        public int DeleteAfterDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum number of tasks to delete in a single batch.
        /// Defaults to 100. Smaller batch sizes reduce database load but increase cleanup duration.
        /// </summary>
        public int BatchSize { get; set; } = 100;
    }

    /// <summary>
    /// Background service that periodically deletes old closed tasks.
    /// Runs as a hosted service in the application and executes cleanup on a configurable schedule.
    /// This helps maintain database performance by removing obsolete task records.
    /// </summary>
    public class ClosedTaskCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClosedTaskCleanupService> _logger;
        private readonly ClosedTaskCleanupOptions _options;
        private Timer? _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClosedTaskCleanupService"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider for creating service scopes.</param>
        /// <param name="logger">Logger instance for recording service activities.</param>
        /// <param name="options">Configuration options for the cleanup service.</param>
        public ClosedTaskCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ClosedTaskCleanupService> logger,
            IOptions<ClosedTaskCleanupOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Executes the cleanup service startup and schedules recurring cleanup operations.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token to stop the service.</param>
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
            var startupDelay = TimeSpan.FromSeconds(Math.Max(0, _options.StartupDelaySeconds));
            await System.Threading.Tasks.Task.Delay(startupDelay, stoppingToken);
            await CleanupOldClosedTasksAsync(stoppingToken);

            // Set up recurring cleanup
            _timer = new Timer(
                async (_) => await CleanupOldClosedTasksAsync(stoppingToken),
                null,
                TimeSpan.FromDays(_options.IntervalDays),
                TimeSpan.FromDays(_options.IntervalDays));
        }

        /// <summary>
        /// Performs the cleanup operation by deleting old closed tasks in batches.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop the cleanup operation.</param>
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

        /// <summary>
        /// Stops the background service and releases the timer resource.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the stop operation.</param>
        public override async System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            _logger.LogInformation("Closed task cleanup service stopped");
            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Disposes the service and releases the timer resource.
        /// </summary>
        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
