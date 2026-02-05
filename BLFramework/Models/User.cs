using System.ComponentModel.DataAnnotations;

namespace BLFramework.Models
{
    /// <summary>
    /// User entity representing application users
    /// </summary>
    public class User : BaseEntity
    {
        [Required]
        [StringLength(32, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 32 characters")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 20, ErrorMessage = "Password hash must be between 20 and 200 characters")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserTask> CreatedTasks { get; set; } = new List<UserTask>();
        public virtual ICollection<Invite> CreatedInvites { get; set; } = new List<Invite>();
        public virtual ICollection<Invite> UsedInvites { get; set; } = new List<Invite>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
