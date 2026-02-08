using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.Models
{
    /// <summary>
    /// Request model for creating a new task.
    /// Contains the task name, optional description, and status assignment.
    /// </summary>
    public class CreateTaskRequest
    {
        /// <summary>
        /// Gets or sets the name of the task.
        /// Must be between 1 and 100 characters and is required.
        /// </summary>
        [Required(ErrorMessage = "Task name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Task name must be between 1 and 100 characters")]
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional description of the task.
        /// Provides additional context or details about what needs to be done.
        /// </summary>
        [StringLength(int.MaxValue, ErrorMessage = "Task description is too long")]
        public string? TaskDescription { get; set; }

        /// <summary>
        /// Gets or sets the status ID for the task.
        /// Must reference a valid existing task status.
        /// </summary>
        [Required(ErrorMessage = "Status ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Status ID must be valid")]
        public int StatusId { get; set; }
    }

    /// <summary>
    /// Request model for updating an existing task.
    /// All fields are optional, only provided fields will be updated.
    /// </summary>
    public class UpdateTaskRequest
    {
        /// <summary>
        /// Gets or sets the new task name.
        /// If provided, must be between 1 and 100 characters.
        /// If null or empty, the task name will not be updated.
        /// </summary>
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Task name must be between 1 and 100 characters")]
        public string? TaskName { get; set; }

        /// <summary>
        /// Gets or sets the new task description.
        /// If provided, replaces the existing description.
        /// If null, the description will not be updated.
        /// </summary>
        public string? TaskDescription { get; set; }

        /// <summary>
        /// Gets or sets the new status ID for the task.
        /// If provided, must reference a valid existing task status.
        /// If null, the task status will not be updated.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Status ID must be valid")]
        public int? StatusId { get; set; }

        /// <summary>
        /// Gets or sets the new priority level within the task status.
        /// If provided, must be non-negative.
        /// If null, the priority will not be updated.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Priority must be non-negative")]
        public int? StatusPriority { get; set; }
    }

    /// <summary>
    /// Data transfer object for task information.
    /// Contains all relevant task details for client responses.
    /// </summary>
    public class TaskDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the task.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the task.
        /// </summary>
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional description of the task.
        /// </summary>
        public string? TaskDescription { get; set; }

        /// <summary>
        /// Gets or sets the ID of the task's current status.
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the status code (e.g., "TODO", "IN_PROGRESS", "DONE").
        /// </summary>
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the user who created the task.
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the task within its status.
        /// Lower numbers indicate higher priority.
        /// </summary>
        public int StatusPriority { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the task was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the task was last modified.
        /// </summary>
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the task was closed, if applicable.
        /// Null if the task has not been closed.
        /// </summary>
        public DateTime? ClosedOn { get; set; }
    }
}
