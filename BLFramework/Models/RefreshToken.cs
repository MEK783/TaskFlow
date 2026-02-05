using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLFramework.Models
{
    /// <summary>
    /// RefreshToken entity representing JWT refresh tokens
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        [Required]
        [StringLength(128, MinimumLength = 20, ErrorMessage = "Token must be between 20 and 128 characters")]
        public string Token { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ExpiresOn { get; set; }

        public DateTime? RevokedOn { get; set; }

        public string? ReplacingToken { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        /// <summary>
        /// Checks if the token is still active (not expired and not revoked)
        /// </summary>
        public bool IsActive => ExpiresOn > DateTime.UtcNow && RevokedOn == null;

        /// <summary>
        /// Checks if the token is expired
        /// </summary>
        public bool IsExpired => ExpiresOn <= DateTime.UtcNow;

        /// <summary>
        /// Checks if the token has been revoked
        /// </summary>
        public bool IsRevoked => RevokedOn != null;
    }
}
