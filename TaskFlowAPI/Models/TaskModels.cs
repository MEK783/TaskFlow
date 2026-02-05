using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.Models
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Task name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Task name must be between 1 and 100 characters")]
        public string TaskName { get; set; } = string.Empty;

        [StringLength(int.MaxValue, ErrorMessage = "Task description is too long")]
        public string? TaskDescription { get; set; }

        [Required(ErrorMessage = "Status ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Status ID must be valid")]
        public int StatusId { get; set; }
    }

    public class UpdateTaskRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Task name must be between 1 and 100 characters")]
        public string? TaskName { get; set; }

        public string? TaskDescription { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Status ID must be valid")]
        public int? StatusId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Priority must be non-negative")]
        public int? StatusPriority { get; set; }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? TaskDescription { get; set; }
        public int StatusId { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public int CreatedById { get; set; }
        public int StatusPriority { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
    }
}
