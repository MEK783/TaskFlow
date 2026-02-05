using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(32, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 32 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; } = false;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Invite code is required")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Invite code must be exactly 16 characters")]
        public string InviteCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(32, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 32 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }

    public class GenerateInviteRequest
    {
        [Display(Name = "Expiration Days")]
        [Range(1, 365, ErrorMessage = "Expiration days must be between 1 and 365")]
        public int ExpirationDays { get; set; } = 15;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InviteDto
    {
        public int Id { get; set; }
        public string InviteCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime? UsedOn { get; set; }
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
    }
}
