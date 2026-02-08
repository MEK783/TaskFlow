using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace BLFramework.Models
{
    /// <summary>
    /// Invite entity representing user invitation codes.
    /// Used to control user registration access through one-time-use invitation codes.
    /// </summary>
    public class Invite : BaseEntity
    {
        /// <summary>
        /// Gets or sets the unique invitation code.
        /// Must be exactly 16 characters, generated using cryptographically secure random data.
        /// </summary>
        [Required]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "InviteCode must be exactly 16 characters")]
        public string InviteCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the user who created this invitation.
        /// Foreign key reference to User entity.
        /// </summary>
        [Required]
        public int CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time of the invitation.
        /// After this time, the invite can no longer be used for registration.
        /// </summary>
        [Required]
        public DateTime ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the invitation was used.
        /// Nullable; remains null if the invite has never been used.
        /// Once set, the invite becomes invalid and cannot be reused.
        /// </summary>
        public DateTime? UsedOn { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who used this invitation for registration.
        /// Foreign key reference to User entity.
        /// Nullable; remains null until the invite is actually used.
        /// </summary>
        public int? UsedById { get; set; }

        /// <summary>
        /// Gets or sets the user who created this invitation.
        /// Navigation property to the related User entity.
        /// </summary>
        [ForeignKey(nameof(CreatedById))]
        public virtual User? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the user who used this invitation for registration.
        /// Navigation property to the related User entity.
        /// Null until the invite has been used.
        /// </summary>
        [ForeignKey(nameof(UsedById))]
        public virtual User? UsedBy { get; set; }

        /// <summary>
        /// Gets a value indicating whether the invitation is still valid.
        /// An invite is valid if it has not expired and has not been used.
        /// </summary>
        public bool IsValid => ExpiresOn > DateTime.UtcNow && UsedOn == null;

        /// <summary>
        /// Gets a value indicating whether the invitation has expired.
        /// Compares the expiration time to the current UTC time.
        /// </summary>
        public bool IsExpired => ExpiresOn <= DateTime.UtcNow;

        /// <summary>
        /// Gets a value indicating whether the invitation has been used.
        /// Returns true if the UsedOn property has been set to a date/time.
        /// </summary>
        public bool IsUsed => UsedOn != null;

        /// <summary>
        /// Generates a random 16-character uppercase alphanumeric invitation code.
        /// Uses cryptographically secure random number generation to ensure uniqueness.
        /// </summary>
        /// <returns>A 16-character invitation code containing uppercase letters A-Z and digits 0-9.</returns>
        public static string GenerateInviteCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int codeLength = 16;
            
            // Use cryptographically secure random number generator
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[codeLength];
                rng.GetBytes(buffer);
                
                var result = new char[codeLength];
                // Map random bytes to valid characters from the allowed character set
                for (int i = 0; i < codeLength; i++)
                {
                    result[i] = chars[buffer[i] % chars.Length];
                }
                
                return new string(result);
            }
        }
    }
}
