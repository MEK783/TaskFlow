using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLFramework.Models
{
    /// <summary>
    /// UserTask entity representing user tasks
    /// </summary>
    public class UserTask : BaseEntity
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "TaskName must be between 1 and 100 characters")]
        public string TaskName { get; set; } = string.Empty;

        public string? TaskDescription { get; set; }

        [Required]
        public int StatusId { get; set; }

        [Required]
        public int CreatedById { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "StatusPriority must be greater than or equal to 0")]
        public int StatusPriority { get; set; }

        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedOn { get; set; }

        // Navigation properties
        [ForeignKey(nameof(StatusId))]
        public virtual TaskStatusDefinition? Status { get; set; }

        [ForeignKey(nameof(CreatedById))]
        public virtual User? CreatedBy { get; set; }

        /// <summary>
        /// Updates the ClosedOn timestamp based on the current status's ClosingStatus flag
        /// </summary>
        public void UpdateClosedStatusBasedOnStatusDefinition()
        {
            if (Status == null)
            {
                return;
            }

            if (Status.ClosingStatus)
            {
                // If closing status and not already closed, mark as closed
                if (ClosedOn == null)
                {
                    ClosedOn = DateTime.UtcNow;
                }
            }
            else
            {
                // If not closing status and currently closed, reopen
                if (ClosedOn != null)
                {
                    ClosedOn = null;
                }
            }
        }
    }
}
