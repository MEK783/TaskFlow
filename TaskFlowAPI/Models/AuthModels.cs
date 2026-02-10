using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.Models
{
    /// <summary>
    /// Request model for user login with username, password, and optional remember-me functionality.
    /// Password should be the SHA512 hash of the actual password sent from the frontend.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Gets or sets the username for login.
        /// Must be between 1 and 32 characters.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(32, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 32 characters")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SHA512 hash of the user's password.
        /// Should be at least 6 characters for the original password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to be remembered.
        /// When true, the refresh token expiration is extended to 30 days.
        /// </summary>
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// Request model for user registration with invite code, username, and password.
    /// An invite code is required to prevent unauthorized registrations.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Gets or sets the invite code for registration.
        /// Must be exactly 16 characters and must be valid and not expired.
        /// </summary>
        [Required(ErrorMessage = "Invite code is required")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Invite code must be exactly 16 characters")]
        public string InviteCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the desired username for the new account.
        /// Must be unique and between 1 and 32 characters.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(32, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 32 characters")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SHA512 hash of the user's password.
        /// Should be at least 6 characters for the original password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for generating a new invite code.
    /// Only authenticated users can generate invites.
    /// </summary>
    public class GenerateInviteRequest
    {
        /// <summary>
        /// Gets or sets the number of days until the invite code expires.
        /// Must be between 1 and 365 days. Defaults to 15 days.
        /// </summary>
        [Display(Name = "Expiration Days")]
        [Range(1, 365, ErrorMessage = "Expiration days must be between 1 and 365")]
        public int ExpirationDays { get; set; } = 15;
    }

    /// <summary>
    /// Response model for authentication operations.
    /// Contains success status, message, and optional user information.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the authentication was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a descriptive message about the authentication result.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the authenticated user information, if successful.
        /// Null if authentication failed.
        /// </summary>
        public UserDto? User { get; set; }
    }

    /// <summary>
    /// Data transfer object for user information.
    /// Contains public user profile information safe to send to clients.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the user's last login.
        /// </summary>
        public DateTime LastLogin { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for invite information.
    /// Contains information about invitation codes for new user registration.
    /// </summary>
    public class InviteDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the invite.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the invite code (16-character alphanumeric string).
        /// </summary>
        public string InviteCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the invite was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the invite expires.
        /// </summary>
        public DateTime ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the invite was used for registration, if applicable.
        /// Null if the invite has not yet been used.
        /// </summary>
        public DateTime? UsedOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the invite is valid and can be used.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the invite has expired.
        /// </summary>
        public bool IsExpired { get; set; }
    }
}
