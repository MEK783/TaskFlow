using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLFramework.Models
{
    /// <summary>
    /// RefreshToken entity representing JWT refresh tokens.
    /// Used for obtaining new access tokens without requiring user re-authentication.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// Gets or sets the refresh token string.
        /// Must be between 20 and 128 characters and should be cryptographically unique.
        /// </summary>
        [Required]
        [StringLength(128, MinimumLength = 20, ErrorMessage = "Token must be between 20 and 128 characters")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the user who owns this refresh token.
        /// Foreign key relationship with the User entity.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time of the refresh token.
        /// After this time, the token can no longer be used to obtain new access tokens.
        /// </summary>
        [Required]
        public DateTime ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the refresh token was revoked.
        /// Nullable; remains null if the token has never been revoked.
        /// </summary>
        public DateTime? RevokedOn { get; set; }

        /// <summary>
        /// Gets or sets the replacement token when this token is refreshed.
        /// Contains the new token string if a refresh operation created a successor token.
        /// </summary>
        public string? ReplacingToken { get; set; }

        /// <summary>
        /// Gets or sets the user who owns this refresh token.
        /// Navigation property to the related User entity.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        /// <summary>
        /// Gets a value indicating whether the token is still active.
        /// A token is active if it has not expired and has not been revoked.
        /// </summary>
        public bool IsActive => ExpiresOn > DateTime.UtcNow && RevokedOn == null;

        /// <summary>
        /// Gets a value indicating whether the token has expired.
        /// Compares the expiration time to the current UTC time.
        /// </summary>
        public bool IsExpired => ExpiresOn <= DateTime.UtcNow;

        /// <summary>
        /// Gets a value indicating whether the token has been revoked.
        /// Returns true if the RevokedOn property has been set to a date/time.
        /// </summary>
        public bool IsRevoked => RevokedOn != null;
    }
}
