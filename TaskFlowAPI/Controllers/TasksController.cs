using BLFramework.Models;
using BLFramework.Services;
using Microsoft.AspNetCore.Mvc;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    /// <summary>
    /// Tasks controller for managing user tasks and their lifecycle.
    /// All task operations require authentication via refresh token.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TasksController"/> class.
        /// </summary>
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
        /// Gets the current authenticated user ID from the refresh token cookie.
        /// </summary>
        /// <returns>The user ID if authenticated, or null if no valid refresh token is found.</returns>
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

        /// <summary>
        /// Retrieves all tasks for the currently authenticated user.
        /// </summary>
        /// <returns>
        /// An HTTP 200 OK response containing a list of all user tasks,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Retrieves a specific task by its ID.
        /// Users can only retrieve their own tasks.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <returns>
        /// An HTTP 200 OK response containing the task details,
        /// HTTP 404 Not Found if the task doesn't exist or belongs to another user,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Retrieves all tasks for the authenticated user filtered by a specific status.
        /// </summary>
        /// <param name="statusId">The ID of the status to filter by.</param>
        /// <returns>
        /// An HTTP 200 OK response containing tasks with the specified status,
        /// HTTP 400 Bad Request if the status ID is invalid,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpGet("status/{statusId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Creates a new task for the authenticated user.
        /// </summary>
        /// <param name="request">The task creation request containing task name, description, and status.</param>
        /// <returns>
        /// An HTTP 201 Created response with the newly created task,
        /// HTTP 400 Bad Request if the status ID is invalid or input is invalid,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Updates an existing task.
        /// Users can only update their own tasks.
        /// </summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="request">The update request containing fields to modify.</param>
        /// <returns>
        /// An HTTP 200 OK response with the updated task,
        /// HTTP 404 Not Found if the task doesn't exist or belongs to another user,
        /// HTTP 400 Bad Request if the status ID is invalid,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Deletes an existing task.
        /// Users can only delete their own tasks.
        /// </summary>
        /// <param name="id">The ID of the task to delete.</param>
        /// <returns>
        /// An HTTP 200 OK response confirming deletion,
        /// HTTP 404 Not Found if the task doesn't exist or belongs to another user,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpDelete("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Closes an existing task, marking it as complete.
        /// Users can only close their own tasks.
        /// </summary>
        /// <param name="id">The ID of the task to close.</param>
        /// <returns>
        /// An HTTP 200 OK response with the closed task,
        /// HTTP 404 Not Found if the task doesn't exist or belongs to another user,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpPatch("{id}/close")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Reopens a previously closed task.
        /// Users can only reopen their own tasks.
        /// </summary>
        /// <param name="id">The ID of the task to reopen.</param>
        /// <returns>
        /// An HTTP 200 OK response with the reopened task,
        /// HTTP 404 Not Found if the task doesn't exist or belongs to another user,
        /// or HTTP 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpPatch("{id}/reopen")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Maps a UserTask entity to a TaskDto for API responses.
        /// </summary>
        /// <param name="task">The task entity to map.</param>
        /// <returns>A TaskDto containing the mapped task information.</returns>
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
