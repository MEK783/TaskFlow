using System.ComponentModel.DataAnnotations;

namespace BLFramework.Models
{
    /// <summary>
    /// TaskStatusDefinition entity representing task status definitions
    /// </summary>
    public class TaskStatusDefinition : BaseEntity
    {
        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "StatusCode must be between 1 and 50 characters")]
        public string StatusCode { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "StatusDescription must not exceed 200 characters")]
        public string? StatusDescription { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "ReactIcon must not exceed 50 characters")]
        [RegularExpression(@"^$|^[a-z][a-z]/[A-Za-z0-9]+$", ErrorMessage = "ReactIcon must be empty or in format 'library/icon' with alphanumeric characters only")]
        public string ReactIcon { get; set; } = string.Empty;

        [Required]
        public bool ClosingStatus { get; set; } = false;

        // Navigation properties
        public virtual ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
    }
}
