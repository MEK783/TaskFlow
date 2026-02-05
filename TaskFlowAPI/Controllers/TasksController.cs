using BLFramework.Models;
using BLFramework.Services;
using Microsoft.AspNetCore.Mvc;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/v1.0/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly TaskStatusService _taskStatusService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly UserService _userService;
        private readonly ILogger<TasksController> _logger;

        private const string RefreshTokenCookieName = "TaskFlowRefreshToken";

        public TasksController(
            TaskService taskService,
            TaskStatusService taskStatusService,
            RefreshTokenService refreshTokenService,
            UserService userService,
            ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _taskStatusService = taskStatusService;
            _refreshTokenService = refreshTokenService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current user ID from the refresh token cookie
        /// </summary>
        private async Task<int?> GetCurrentUserIdAsync()
        {
            if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshTokenValue))
            {
                return null;
            }

            var refreshToken = await _refreshTokenService.GetByTokenAsync(refreshTokenValue);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                return null;
            }

            return refreshToken.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasksAsync()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                var tasks = await _taskService.GetTasksByUserAsync(userId.Value);
                var taskDtos = tasks.Select(t => MapTaskToDto(t)).ToList();

                return Ok(new
                {
                    success = true,
                    message = "Tasks retrieved successfully",
                    tasks = taskDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving tasks" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskByIdAsync(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                var task = await _taskService.GetByIdWithDetailsAsync(id);
                if (task == null || task.CreatedById != userId.Value)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Task retrieved successfully",
                    task = MapTaskToDto(task)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task {TaskId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving task" });
            }
        }

        [HttpGet("status/{statusId}")]
        public async Task<IActionResult> GetTasksByStatusAsync(int statusId)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                // Verify status exists
                var status = await _taskStatusService.GetByIdAsync(statusId);
                if (status == null)
                {
                    return BadRequest(new { success = false, message = "Invalid status ID" });
                }

                var tasks = await _taskService.GetTasksByStatusAndUserAsync(statusId, userId.Value);
                var taskDtos = tasks.Select(t => MapTaskToDto(t)).ToList();

                return Ok(new
                {
                    success = true,
                    message = "Tasks retrieved successfully",
                    tasks = taskDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks by status");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving tasks" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskAsync([FromBody] CreateTaskRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors) });
                }

                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                // Verify status exists
                var status = await _taskStatusService.GetByIdAsync(request.StatusId);
                if (status == null)
                {
                    return BadRequest(new { success = false, message = "Invalid status ID" });
                }

                // Get next priority for this status and user
                var nextPriority = await _taskService.GetNextPriorityAsync(request.StatusId, userId.Value);

                var newTask = new UserTask
                {
                    TaskName = request.TaskName,
                    TaskDescription = request.TaskDescription,
                    StatusId = request.StatusId,
                    CreatedById = userId.Value,
                    StatusPriority = nextPriority,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };

                var createdTask = await _taskService.AddAsync(newTask);

                _logger.LogInformation("Task {TaskId} created by user {UserId}", createdTask.Id, userId.Value);

                return CreatedAtAction(nameof(GetTaskByIdAsync), new { id = createdTask.Id }, new
                {
                    success = true,
                    message = "Task created successfully",
                    task = MapTaskToDto(createdTask)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, new { success = false, message = "An error occurred while creating task" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskAsync(int id, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                var task = await _taskService.GetByIdAsync(id);
                if (task == null || task.CreatedById != userId.Value)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }

                // Update task properties if provided
                if (!string.IsNullOrWhiteSpace(request.TaskName))
                {
                    task.TaskName = request.TaskName;
                }

                if (request.TaskDescription != null)
                {
                    task.TaskDescription = request.TaskDescription;
                }

                if (request.StatusId.HasValue && request.StatusId.Value != task.StatusId)
                {
                    var status = await _taskStatusService.GetByIdAsync(request.StatusId.Value);
                    if (status == null)
                    {
                        return BadRequest(new { success = false, message = "Invalid status ID" });
                    }
                    task.StatusId = request.StatusId.Value;
                }

                if (request.StatusPriority.HasValue)
                {
                    task.StatusPriority = request.StatusPriority.Value;
                }

                task.ModifiedOn = DateTime.UtcNow;
                var updatedTask = await _taskService.UpdateAsync(task);

                _logger.LogInformation("Task {TaskId} updated by user {UserId}", id, userId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Task updated successfully",
                    task = MapTaskToDto(updatedTask)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while updating task" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskAsync(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                var task = await _taskService.GetByIdAsync(id);
                if (task == null || task.CreatedById != userId.Value)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }

                await _taskService.DeleteAsync(task);

                _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, userId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Task deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting task" });
            }
        }

        [HttpPatch("{id}/close")]
        public async Task<IActionResult> CloseTaskAsync(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                var task = await _taskService.GetByIdAsync(id);
                if (task == null || task.CreatedById != userId.Value)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }

                var closedTask = await _taskService.CloseTaskAsync(id);

                _logger.LogInformation("Task {TaskId} closed by user {UserId}", id, userId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Task closed successfully",
                    task = MapTaskToDto(closedTask)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing task {TaskId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while closing task" });
            }
        }

        [HttpPatch("{id}/reopen")]
        public async Task<IActionResult> ReopenTaskAsync(int id)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                var task = await _taskService.GetByIdAsync(id);
                if (task == null || task.CreatedById != userId.Value)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }

                var reopenedTask = await _taskService.ReopenTaskAsync(id);

                _logger.LogInformation("Task {TaskId} reopened by user {UserId}", id, userId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Task reopened successfully",
                    task = MapTaskToDto(reopenedTask)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reopening task {TaskId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while reopening task" });
            }
        }

        private TaskDto MapTaskToDto(UserTask task)
        {
            return new TaskDto
            {
                Id = task.Id,
                TaskName = task.TaskName,
                TaskDescription = task.TaskDescription,
                StatusId = task.StatusId,
                StatusCode = task.Status?.StatusCode ?? string.Empty,
                CreatedById = task.CreatedById,
                StatusPriority = task.StatusPriority,
                CreatedOn = task.CreatedAt,
                ModifiedOn = task.ModifiedOn,
                ClosedOn = task.ClosedOn
            };
        }
    }
}
