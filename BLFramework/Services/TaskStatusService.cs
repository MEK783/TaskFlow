using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for TaskStatusDefinition entity operations.
    /// Provides read-only access to task status definitions.
    /// TaskStatus is a reference table managed through database scripts and cannot be modified through this service.
    /// </summary>
    public class TaskStatusService : BaseService<TaskStatusDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskStatusService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger for service operations.</param>
        public TaskStatusService(AppDbContext context, ILogger<BaseService<TaskStatusDefinition>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves a task status by its status code asynchronously.
        /// </summary>
        /// <param name="statusCode">The status code to search for.</param>
        /// <returns>The TaskStatusDefinition if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown if statusCode is null or empty.</exception>
        public async System.Threading.Tasks.Task<TaskStatusDefinition?> GetByStatusCodeAsync(string statusCode)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(statusCode))
                {
                    throw new ArgumentException("StatusCode cannot be null or empty", nameof(statusCode));
                }

                // Query by StatusCode
                return await _context.TaskStatusDefinitions.FirstOrDefaultAsync(ts => ts.StatusCode == statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task status by code {StatusCode}", statusCode);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all status definitions marked as closing statuses asynchronously.
        /// Closing statuses indicate tasks that are completed and cannot be reopened.
        /// </summary>
        /// <returns>A list of all closing status definitions.</returns>
        public async System.Threading.Tasks.Task<List<TaskStatusDefinition>> GetClosingStatusesAsync()
        {
            try
            {
                // Get statuses where ClosingStatus is true
                return await _context.TaskStatusDefinitions
                    .Where(ts => ts.ClosingStatus)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving closing statuses");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all status definitions not marked as closing statuses asynchronously.
        /// Open statuses allow tasks to remain active and changeable.
        /// </summary>
        /// <returns>A list of all open (non-closing) status definitions.</returns>
        public async System.Threading.Tasks.Task<List<TaskStatusDefinition>> GetOpenStatusesAsync()
        {
            try
            {
                // Get statuses where ClosingStatus is false
                return await _context.TaskStatusDefinitions
                    .Where(ts => !ts.ClosingStatus)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving open statuses");
                throw;
            }
        }

        /// <summary>
        /// Checks if a status code already exists asynchronously.
        /// </summary>
        /// <param name="statusCode">The status code to check.</param>
        /// <returns>True if the status code exists; otherwise, false.</returns>
        public async System.Threading.Tasks.Task<bool> StatusCodeExistsAsync(string statusCode)
        {
            try
            {
                // Return false for null/empty input
                if (string.IsNullOrWhiteSpace(statusCode))
                {
                    return false;
                }

                // Check if any status has this code
                return await _context.TaskStatusDefinitions.AnyAsync(ts => ts.StatusCode == statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if status code exists {StatusCode}", statusCode);
                throw;
            }
        }

        /// <summary>
        /// TaskStatus is a read-only reference table populated through database scripts.
        /// Addition of new statuses is not permitted through this service.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown - TaskStatus cannot be modified.</exception>
        public override async System.Threading.Tasks.Task<TaskStatusDefinition> AddAsync(TaskStatusDefinition entity)
        {
            throw new NotSupportedException("TaskStatus is a read-only reference table and cannot be modified.");
        }

        /// <summary>
        /// TaskStatus is a read-only reference table populated through database scripts.
        /// Updates to statuses are not permitted through this service.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown - TaskStatus cannot be modified.</exception>
        public override async System.Threading.Tasks.Task<TaskStatusDefinition> UpdateAsync(TaskStatusDefinition entity)
        {
            throw new NotSupportedException("TaskStatus is a read-only reference table and cannot be modified.");
        }

        /// <summary>
        /// TaskStatus is a read-only reference table populated through database scripts.
        /// Deletion of statuses is not permitted through this service.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown - TaskStatus cannot be modified.</exception>
        public override async System.Threading.Tasks.Task DeleteAsync(TaskStatusDefinition entity)
        {
            throw new NotSupportedException("TaskStatus is a read-only reference table and cannot be modified.");
        }
    }
}
