using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for UserTask entity operations
    /// </summary>
    public class TaskService : BaseService<UserTask>
    {
        public TaskService(AppDbContext context, ILogger<BaseService<UserTask>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Gets all tasks with related data
        /// </summary>
        public async System.Threading.Tasks.Task<List<UserTask>> GetAllWithDetailsAsync()
        {
            try
            {
                return await _context.Tasks
                    .Include(t => t.Status)
                    .Include(t => t.CreatedBy)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tasks with details");
                throw;
            }
        }

        /// <summary>
        /// Gets a task by ID with related data
        /// </summary>
        public async System.Threading.Tasks.Task<UserTask?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.Tasks
                    .Include(t => t.Status)
                    .Include(t => t.CreatedBy)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId} with details", id);
                throw;
            }
        }

        /// <summary>
        /// Gets all tasks created by a specific user
        /// </summary>
        public async System.Threading.Tasks.Task<List<UserTask>> GetTasksByUserAsync(int userId)
        {
            try
            {
                return await _context.Tasks
                    .Where(t => t.CreatedById == userId)
                    .Include(t => t.Status)
                    .OrderBy(t => t.StatusPriority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets tasks by status for a specific user
        /// </summary>
        public async System.Threading.Tasks.Task<List<UserTask>> GetTasksByStatusAndUserAsync(int statusId, int userId)
        {
            try
            {
                return await _context.Tasks
                    .Where(t => t.StatusId == statusId && t.CreatedById == userId)
                    .Include(t => t.Status)
                    .OrderBy(t => t.StatusPriority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks by status {StatusId} for user {UserId}", statusId, userId);
                throw;
            }
        }

        /// <summary>
        /// Gets open tasks for a specific user
        /// </summary>
        public async System.Threading.Tasks.Task<List<UserTask>> GetOpenTasksForUserAsync(int userId)
        {
            try
            {
                return await _context.Tasks
                    .Where(t => t.CreatedById == userId && t.ClosedOn == null)
                    .Include(t => t.Status)
                    .OrderBy(t => t.StatusPriority)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving open tasks for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets closed tasks for a specific user
        /// </summary>
        public async System.Threading.Tasks.Task<List<UserTask>> GetClosedTasksForUserAsync(int userId)
        {
            try
            {
                return await _context.Tasks
                    .Where(t => t.CreatedById == userId && t.ClosedOn != null)
                    .Include(t => t.Status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving closed tasks for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Checks if a task name already exists for a user (uniqueness constraint)
        /// </summary>
        public async Task<bool> TaskNameExistsForUserAsync(string taskName, int userId, int? excludeTaskId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taskName))
                {
                    return false;
                }

                var query = _context.Tasks.Where(t => t.TaskName == taskName && t.CreatedById == userId);
                
                if (excludeTaskId.HasValue)
                {
                    query = query.Where(t => t.Id != excludeTaskId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if task name exists {TaskName} for user {UserId}", taskName, userId);
                throw;
            }
        }

        /// <summary>
        /// Gets the next available priority for a task status and user
        /// </summary>
        public async System.Threading.Tasks.Task<int> GetNextPriorityAsync(int statusId, int userId)
        {
            try
            {
                var maxPriority = await _context.Tasks
                    .Where(t => t.StatusId == statusId && t.CreatedById == userId)
                    .MaxAsync(t => (int?)t.StatusPriority) ?? -1;

                return maxPriority + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next priority for status {StatusId} and user {UserId}", statusId, userId);
                throw;
            }
        }

        /// <summary>
        /// Changes task status and priority
        /// </summary>
        public async System.Threading.Tasks.Task<UserTask> ChangeStatusAsync(int taskId, int newStatusId, int newPriority)
        {
            try
            {
                var task = await GetByIdWithDetailsAsync(taskId);
                if (task == null)
                {
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                // Verify the new status exists
                var newStatus = await _context.TaskStatusDefinitions.FirstOrDefaultAsync(s => s.Id == newStatusId);
                if (newStatus == null)
                {
                    throw new ValidationException($"TaskStatus with ID {newStatusId} does not exist");
                }

                // Update status and priority
                task.StatusId = newStatusId;
                task.Status = newStatus;
                task.StatusPriority = newPriority;
                task.ModifiedOn = DateTime.UtcNow;

                // Update closed status based on the new status's ClosingStatus flag
                task.UpdateClosedStatusBasedOnStatusDefinition();

                return await UpdateAsync(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing status for task {TaskId}", taskId);
                throw;
            }
        }

        /// <summary>
        /// Closes a task (sets ClosedOn timestamp)
        /// </summary>
        public async System.Threading.Tasks.Task<UserTask> CloseTaskAsync(int taskId)
        {
            try
            {
                var task = await GetByIdAsync(taskId);
                if (task == null)
                {
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                task.ClosedOn = DateTime.UtcNow;
                task.ModifiedOn = DateTime.UtcNow;

                return await UpdateAsync(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing task {TaskId}", taskId);
                throw;
            }
        }

        /// <summary>
        /// Reopens a task (clears ClosedOn timestamp)
        /// </summary>
        public async System.Threading.Tasks.Task<UserTask> ReopenTaskAsync(int taskId)
        {
            try
            {
                var task = await GetByIdAsync(taskId);
                if (task == null)
                {
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                task.ClosedOn = null;
                task.ModifiedOn = DateTime.UtcNow;

                return await UpdateAsync(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reopening task {TaskId}", taskId);
                throw;
            }
        }

        public override async System.Threading.Tasks.Task<UserTask> AddAsync(UserTask entity)
        {
            try
            {
                // Validate foreign keys exist  
                var status = await _context.TaskStatusDefinitions.FirstOrDefaultAsync(ts => ts.Id == entity.StatusId);
                if (status == null)
                {
                    throw new ValidationException($"TaskStatus with ID {entity.StatusId} does not exist");
                }

                var userExists = await _context.Users.AnyAsync(u => u.Id == entity.CreatedById);
                if (!userExists)
                {
                    throw new ValidationException($"User with ID {entity.CreatedById} does not exist");
                }

                // Check unique constraint: TaskName + CreatedBy
                if (await TaskNameExistsForUserAsync(entity.TaskName, entity.CreatedById))
                {
                    throw new ValidationException($"A task with name '{entity.TaskName}' already exists for this user");
                }

                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = null;
                entity.ModifiedOn = DateTime.UtcNow;
                entity.Status = status;

                // Update closed status based on the initial status's ClosingStatus flag
                entity.UpdateClosedStatusBasedOnStatusDefinition();

                return await base.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding task");
                throw;
            }
        }

        public override async System.Threading.Tasks.Task<UserTask> UpdateAsync(UserTask entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                entity.ModifiedOn = DateTime.UtcNow;

                // Load the status if it's being updated to ensure ClosedOn is set correctly
                if (entity.Status == null)
                {
                    var status = await _context.TaskStatusDefinitions.FirstOrDefaultAsync(s => s.Id == entity.StatusId);
                    if (status != null)
                    {
                        entity.Status = status;
                        entity.UpdateClosedStatusBasedOnStatusDefinition();
                    }
                }
                else
                {
                    // Status is already loaded, just update based on it
                    entity.UpdateClosedStatusBasedOnStatusDefinition();
                }

                return await base.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task");
                throw;
            }
        }
    }
}
