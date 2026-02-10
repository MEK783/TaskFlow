using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for UserTask entity operations.
    /// Manages user task CRUD operations, status transitions, and business logic enforcement.
    /// Handles task closing/reopening, priority management, and status change validation.
    /// </summary>
    public class TaskService : BaseService<UserTask>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger for service operations.</param>
        public TaskService(AppDbContext context, ILogger<BaseService<UserTask>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves all tasks with related entity data asynchronously.
        /// Eager loads Status and CreatedBy navigation properties.
        /// </summary>
        /// <returns>A list of all tasks with their related status and creator information.</returns>
        public async System.Threading.Tasks.Task<List<UserTask>> GetAllWithDetailsAsync()
        {
            try
            {
                // Eager load Status and CreatedBy navigation properties to avoid N+1 queries
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
        /// Retrieves a task by ID with related entity data asynchronously.
        /// Eager loads Status and CreatedBy navigation properties.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <returns>The task with its related status and creator information if found; otherwise, null.</returns>
        public async System.Threading.Tasks.Task<UserTask?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                // Eager load Status and CreatedBy to provide complete task context
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
        /// Retrieves all tasks created by a specific user asynchronously.
        /// Results are ordered by StatusPriority for display order.
        /// </summary>
        /// <param name="userId">The ID of the user who created the tasks.</param>
        /// <returns>A list of tasks created by the specified user, ordered by priority.</returns>
        public async System.Threading.Tasks.Task<List<UserTask>> GetTasksByUserAsync(int userId)
        {
            try
            {
                // Get all tasks for the user, include status details, order by priority
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
        /// Retrieves tasks by status for a specific user asynchronously.
        /// Results are ordered by StatusPriority within the status.
        /// </summary>
        /// <param name="statusId">The ID of the task status to filter by.</param>
        /// <param name="userId">The ID of the user who created the tasks.</param>
        /// <returns>A list of tasks matching the status and user, ordered by priority.</returns>
        public async System.Threading.Tasks.Task<List<UserTask>> GetTasksByStatusAndUserAsync(int statusId, int userId)
        {
            try
            {
                // Get tasks for a specific user/status combination
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
        /// Retrieves all open (non-closed) tasks for a specific user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user who created the tasks.</param>
        /// <returns>A list of open tasks for the specified user, ordered by priority.</returns>
        public async System.Threading.Tasks.Task<List<UserTask>> GetOpenTasksForUserAsync(int userId)
        {
            try
            {
                // Get tasks where ClosedOn is null (still active)
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
        /// Retrieves all closed tasks for a specific user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user who created the tasks.</param>
        /// <returns>A list of closed tasks for the specified user.</returns>
        public async System.Threading.Tasks.Task<List<UserTask>> GetClosedTasksForUserAsync(int userId)
        {
            try
            {
                // Get tasks where ClosedOn is not null (completed/closed)
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
        /// Checks if a task name already exists for a user asynchronously.
        /// Enforces the uniqueness constraint on TaskName per user.
        /// </summary>
        /// <param name="taskName">The task name to check.</param>
        /// <param name="userId">The ID of the user who would own the task.</param>
        /// <param name="excludeTaskId">Optional task ID to exclude from the check (for updates).</param>
        /// <returns>True if a task with the name exists for the user; otherwise, false.</returns>
        public async Task<bool> TaskNameExistsForUserAsync(string taskName, int userId, int? excludeTaskId = null)
        {
            try
            {
                // Return false for null/empty input
                if (string.IsNullOrWhiteSpace(taskName))
                {
                    return false;
                }

                // Query for tasks matching the name and user
                var query = _context.Tasks.Where(t => t.TaskName == taskName && t.CreatedById == userId);
                
                // Exclude the specified task ID if updating an existing task
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
        /// Gets the next available priority value for a task within a status and user asynchronously.
        /// </summary>
        /// <param name="statusId">The ID of the task status.</param>
        /// <param name="userId">The ID of the user creating the task.</param>
        /// <returns>The next priority value (max existing + 1, or 0 if no tasks exist).</returns>
        public async System.Threading.Tasks.Task<int> GetNextPriorityAsync(int statusId, int userId)
        {
            try
            {
                // Find the maximum priority for the user/status combination
                var maxPriority = await _context.Tasks
                    .Where(t => t.StatusId == statusId && t.CreatedById == userId)
                    .MaxAsync(t => (int?)t.StatusPriority) ?? -1;

                // Return the next priority (max + 1)
                return maxPriority + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next priority for status {StatusId} and user {UserId}", statusId, userId);
                throw;
            }
        }

        /// <summary>
        /// Changes a task's status and priority asynchronously.
        /// Validates the new status exists and updates the task's closed status accordingly.
        /// </summary>
        /// <param name="taskId">The ID of the task to update.</param>
        /// <param name="newStatusId">The ID of the new status.</param>
        /// <param name="newPriority">The new priority value within the status.</param>
        /// <returns>The updated task.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the task is not found.</exception>
        /// <exception cref="ValidationException">Thrown if the new status does not exist.</exception>
        public async System.Threading.Tasks.Task<UserTask> ChangeStatusAsync(int taskId, int newStatusId, int newPriority)
        {
            try
            {
                // Retrieve the task with its navigation properties
                var task = await GetByIdWithDetailsAsync(taskId);
                if (task == null)
                {
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                // Verify the new status exists in the database
                var newStatus = await _context.TaskStatusDefinitions.FirstOrDefaultAsync(s => s.Id == newStatusId);
                if (newStatus == null)
                {
                    throw new ValidationException($"TaskStatus with ID {newStatusId} does not exist");
                }

                // Update task status, priority, and modification time
                task.StatusId = newStatusId;
                task.Status = newStatus;
                task.StatusPriority = newPriority;
                task.ModifiedOn = DateTime.UtcNow;

                // Update the ClosedOn timestamp based on the new status's ClosingStatus flag
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
        /// Closes a task by setting its ClosedOn timestamp to current UTC time asynchronously.
        /// </summary>
        /// <param name="taskId">The ID of the task to close.</param>
        /// <returns>The closed task.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the task is not found.</exception>
        public async System.Threading.Tasks.Task<UserTask> CloseTaskAsync(int taskId)
        {
            try
            {
                // Retrieve the task
                var task = await GetByIdAsync(taskId);
                if (task == null)
                {
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                // Set ClosedOn to current UTC time and update ModifiedOn
                task.ClosedOn = DateTime.UtcNow;
                task.ModifiedOn = DateTime.UtcNow;

                // Inline the update logic to ensure ClosedOn is saved
                var entry = _context.Entry(task);
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                {
                    _context.Set<UserTask>().Update(task);
                }
                await _context.SaveChangesAsync();
                
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing task {TaskId}", taskId);
                throw;
            }
        }

        /// <summary>
        /// Reopens a task by clearing its ClosedOn timestamp asynchronously.
        /// </summary>
        /// <param name="taskId">The ID of the task to reopen.</param>
        /// <returns>The reopened task.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the task is not found.</exception>
        public async System.Threading.Tasks.Task<UserTask> ReopenTaskAsync(int taskId)
        {
            try
            {
                // Retrieve the task
                var task = await GetByIdAsync(taskId);
                if (task == null)
                {
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                // Clear ClosedOn to reopen the task and update ModifiedOn
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

        /// <summary>
        /// Overrides base AddAsync to validate foreign keys and enforce business rules.
        /// </summary>
        /// <param name="entity">The task entity to add.</param>
        /// <returns>The created task.</returns>
        /// <exception cref="ValidationException">Thrown if validation fails.</exception>
        public override async System.Threading.Tasks.Task<UserTask> AddAsync(UserTask entity)
        {
            try
            {
                // Validate that the status exists
                var status = await _context.TaskStatusDefinitions.FirstOrDefaultAsync(ts => ts.Id == entity.StatusId);
                if (status == null)
                {
                    throw new ValidationException($"TaskStatus with ID {entity.StatusId} does not exist");
                }

                // Validate that the user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == entity.CreatedById);
                if (!userExists)
                {
                    throw new ValidationException($"User with ID {entity.CreatedById} does not exist");
                }

                // Enforce unique constraint: TaskName per user
                if (await TaskNameExistsForUserAsync(entity.TaskName, entity.CreatedById))
                {
                    throw new ValidationException($"A task with name '{entity.TaskName}' already exists for this user");
                }

                // Set audit fields
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

        /// <summary>
        /// Overrides base UpdateAsync to set audit fields and update closed status.
        /// </summary>
        /// <param name="entity">The task entity with updated values.</param>
        /// <returns>The updated task.</returns>
        public override async System.Threading.Tasks.Task<UserTask> UpdateAsync(UserTask entity)
        {
            try
            {
                // Update the modification timestamps
                entity.UpdatedAt = DateTime.UtcNow;
                entity.ModifiedOn = DateTime.UtcNow;

                // Load the status if not already loaded to update ClosedOn correctly
                if (entity.Status == null)
                {
                    var status = await _context.TaskStatusDefinitions.FirstOrDefaultAsync(s => s.Id == entity.StatusId);
                    if (status != null)
                    {
                        entity.Status = status;
                        // Update ClosedOn based on the status's ClosingStatus flag
                        entity.UpdateClosedStatusBasedOnStatusDefinition();
                    }
                }
                else
                {
                    // Status is already loaded, update ClosedOn based on it
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
