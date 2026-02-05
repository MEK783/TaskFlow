using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace BLFramework.Models
{
    /// <summary>
    /// Invite entity representing user invitation codes
    /// </summary>
    public class Invite : BaseEntity
    {
        [Required]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "InviteCode must be exactly 16 characters")]
        public string InviteCode { get; set; } = string.Empty;

        [Required]
        public int CreatedById { get; set; }

        [Required]
        public DateTime ExpiresOn { get; set; }

        public DateTime? UsedOn { get; set; }

        public int? UsedById { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CreatedById))]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey(nameof(UsedById))]
        public virtual User? UsedBy { get; set; }

        /// <summary>
        /// Checks if the invite is still valid (not expired and not used)
        /// </summary>
        public bool IsValid => ExpiresOn > DateTime.UtcNow && UsedOn == null;

        /// <summary>
        /// Checks if the invite is expired
        /// </summary>
        public bool IsExpired => ExpiresOn <= DateTime.UtcNow;

        /// <summary>
        /// Checks if the invite has been used
        /// </summary>
        public bool IsUsed => UsedOn != null;

        /// <summary>
        /// Generates a random 16-character uppercase alphanumeric invite code
        /// </summary>
        public static string GenerateInviteCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int codeLength = 16;
            
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[codeLength];
                rng.GetBytes(buffer);
                
                var result = new char[codeLength];
                for (int i = 0; i < codeLength; i++)
                {
                    result[i] = chars[buffer[i] % chars.Length];
                }
                
                return new string(result);
            }
        }
    }
}
