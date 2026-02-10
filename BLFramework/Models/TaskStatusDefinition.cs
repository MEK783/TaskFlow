using System.ComponentModel.DataAnnotations;

namespace BLFramework.Models
{
    /// <summary>
    /// TaskStatusDefinition entity representing task status definitions.
    /// Defines the possible statuses that tasks can have in the system.
    /// </summary>
    public class TaskStatusDefinition : BaseEntity
    {
        /// <summary>
        /// Gets or sets the unique status code identifier.
        /// Used internally to identify the status, between 1 and 50 characters.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "StatusCode must be between 1 and 50 characters")]
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable description of the status.
        /// Provides additional context about what the status represents, maximum 200 characters.
        /// </summary>
        [StringLength(200, ErrorMessage = "StatusDescription must not exceed 200 characters")]
        public string? StatusDescription { get; set; }

        /// <summary>
        /// Gets or sets the React icon identifier for the status.
        /// Format: 'library/icon' (e.g., 'ai/checkCircle', 'md/alertCircle').
        /// Used for UI display of status indicators.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "ReactIcon must not exceed 50 characters")]
        [RegularExpression(@"^$|^[a-z][a-z]/[A-Za-z0-9]+$", ErrorMessage = "ReactIcon must be empty or in format 'library/icon' with alphanumeric characters only")]
        public string ReactIcon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this status represents a closed/completed task.
        /// Tasks with closing statuses cannot be reopened and are hidden from active task lists.
        /// </summary>
        [Required]
        public bool ClosingStatus { get; set; } = false;

        /// <summary>
        /// Gets or sets the collection of tasks that have this status.
        /// Navigation property for the many-to-one relationship with UserTask.
        /// </summary>
        public virtual ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
    }
}
