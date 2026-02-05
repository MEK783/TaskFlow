using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for TaskStatusDefinition entity operations
    /// </summary>
    public class TaskStatusService : BaseService<TaskStatusDefinition>
    {
        public TaskStatusService(AppDbContext context, ILogger<BaseService<TaskStatusDefinition>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Gets a task status by status code
        /// </summary>
        public async System.Threading.Tasks.Task<TaskStatusDefinition?> GetByStatusCodeAsync(string statusCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statusCode))
                {
                    throw new ArgumentException("StatusCode cannot be null or empty", nameof(statusCode));
                }

                return await _context.TaskStatusDefinitions.FirstOrDefaultAsync(ts => ts.StatusCode == statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task status by code {StatusCode}", statusCode);
                throw;
            }
        }

        /// <summary>
        /// Gets all closing statuses
        /// </summary>
        public async System.Threading.Tasks.Task<List<TaskStatusDefinition>> GetClosingStatusesAsync()
        {
            try
            {
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
        /// Gets all non-closing statuses
        /// </summary>
        public async System.Threading.Tasks.Task<List<TaskStatusDefinition>> GetOpenStatusesAsync()
        {
            try
            {
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
        /// Checks if a status code already exists
        /// </summary>
        public async System.Threading.Tasks.Task<bool> StatusCodeExistsAsync(string statusCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statusCode))
                {
                    return false;
                }

                return await _context.TaskStatusDefinitions.AnyAsync(ts => ts.StatusCode == statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if status code exists {StatusCode}", statusCode);
                throw;
            }
        }

        /// <summary>
        /// TaskStatus is a read-only reference table. Adding is not permitted.
        /// </summary>
        public override async System.Threading.Tasks.Task<TaskStatusDefinition> AddAsync(TaskStatusDefinition entity)
        {
            throw new NotSupportedException("TaskStatus is a read-only reference table and cannot be modified.");
        }

        /// <summary>
        /// TaskStatus is a read-only reference table. Updates are not permitted.
        /// </summary>
        public override async System.Threading.Tasks.Task<TaskStatusDefinition> UpdateAsync(TaskStatusDefinition entity)
        {
            throw new NotSupportedException("TaskStatus is a read-only reference table and cannot be modified.");
        }

        /// <summary>
        /// TaskStatus is a read-only reference table. Deletion is not permitted.
        /// </summary>
        public override async System.Threading.Tasks.Task DeleteAsync(TaskStatusDefinition entity)
        {
            throw new NotSupportedException("TaskStatus is a read-only reference table and cannot be modified.");
        }
    }
}
