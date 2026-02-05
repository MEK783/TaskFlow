using System.Text;
using Isopoh.Cryptography.Argon2;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for password hashing and verification using double hashing approach:
    /// Frontend: SHA512 hash of password
    /// Backend: Argon2id hash of the SHA512 hash
    /// </summary>
    public static class PasswordHashingService
    {
        /// <summary>
        /// Hashes a SHA512 password hash using Argon2id.
        /// The input should be the SHA512 hash received from the frontend.
        /// </summary>
        public static string HashPassword(string sha512Hash)
        {
            try
            {
                // Use Argon2 static Hash method with the SHA512 hash from the frontend
                var argon2String = Argon2.Hash(sha512Hash);
                return argon2String;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error hashing password", ex);
            }
        }

        /// <summary>
        /// Verifies a SHA512 password hash against an Argon2id hash.
        /// The input should be the SHA512 hash received from the frontend.
        /// </summary>
        public static bool VerifyPassword(string sha512Hash, string argon2Hash)
        {
            try
            {
                // Verify using Argon2 static method
                var verified = Argon2.Verify(argon2Hash, sha512Hash);
                return verified;
            }
            catch
            {
                return false;
            }
        }
    }
}
