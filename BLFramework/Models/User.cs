using System.ComponentModel.DataAnnotations;

namespace BLFramework.Models
{
    /// <summary>
    /// User entity representing application users.
    /// Stores user credentials and manages user authentication state.
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// Gets or sets the username for the user.
        /// Must be unique, between 1 and 32 characters.
        /// </summary>
        [Required]
        [StringLength(32, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 32 characters")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Argon2id hashed password.
        /// Stores the result of double-hashing: SHA512 (frontend) then Argon2id (backend).
        /// Must be between 20 and 200 characters.
        /// </summary>
        [Required]
        [StringLength(200, MinimumLength = 20, ErrorMessage = "Password hash must be between 20 and 200 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user account is active.
        /// Inactive users cannot authenticate or perform any operations.
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the date and time of the user's last successful login.
        /// Updated whenever the user successfully authenticates.
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the collection of tasks created by this user.
        /// </summary>
        public virtual ICollection<UserTask> CreatedTasks { get; set; } = new List<UserTask>();

        /// <summary>
        /// Gets or sets the collection of invites created by this user.
        /// </summary>
        public virtual ICollection<Invite> CreatedInvites { get; set; } = new List<Invite>();

        /// <summary>
        /// Gets or sets the collection of invites used by this user for registration.
        /// </summary>
        public virtual ICollection<Invite> UsedInvites { get; set; } = new List<Invite>();

        /// <summary>
        /// Gets or sets the collection of refresh tokens issued to this user.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
