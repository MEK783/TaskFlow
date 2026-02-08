using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLFramework.Models
{
    /// <summary>
    /// UserTask entity representing user tasks.
    /// Contains task information including name, description, status, and assignment details.
    /// </summary>
    public class UserTask : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name of the task.
        /// Must be between 1 and 100 characters and provides a brief title for the task.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "TaskName must be between 1 and 100 characters")]
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed description of the task.
        /// Optional field that may contain additional context or instructions.
        /// </summary>
        public string? TaskDescription { get; set; }

        /// <summary>
        /// Gets or sets the ID of the task status.
        /// Foreign key reference to TaskStatusDefinition entity.
        /// </summary>
        [Required]
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this task.
        /// Foreign key reference to User entity.
        /// </summary>
        [Required]
        public int CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the task.
        /// Higher numbers indicate higher priority. Must be greater than or equal to 0.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "StatusPriority must be greater than or equal to 0")]
        public int StatusPriority { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the task was last modified.
        /// Updated whenever task properties change (status, priority, etc.).
        /// </summary>
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the task was closed.
        /// Nullable; remains null for active tasks and is set when task reaches a closing status.
        /// </summary>
        public DateTime? ClosedOn { get; set; }

        /// <summary>
        /// Gets or sets the task status definition.
        /// Navigation property to the related TaskStatusDefinition entity.
        /// </summary>
        [ForeignKey(nameof(StatusId))]
        public virtual TaskStatusDefinition? Status { get; set; }

        /// <summary>
        /// Gets or sets the user who created this task.
        /// Navigation property to the related User entity.
        /// </summary>
        [ForeignKey(nameof(CreatedById))]
        public virtual User? CreatedBy { get; set; }

        /// <summary>
        /// Updates the ClosedOn timestamp based on the current status's ClosingStatus flag.
        /// If the status is a closing status and ClosedOn is null, sets it to current UTC time.
        /// If the status is not a closing status and ClosedOn is set, clears it (reopens the task).
        /// </summary>
        public void UpdateClosedStatusBasedOnStatusDefinition()
        {
            // Guard clause: exit if status navigation property is not loaded
            if (Status == null)
            {
                return;
            }

            if (Status.ClosingStatus)
            {
                // If closing status and not already closed, mark as closed now
                if (ClosedOn == null)
                {
                    ClosedOn = DateTime.UtcNow;
                }
            }
            else
            {
                // If not closing status and currently marked closed, reopen by clearing ClosedOn
                if (ClosedOn != null)
                {
                    ClosedOn = null;
                }
            }
        }
    }
}
