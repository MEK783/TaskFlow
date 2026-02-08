using System.Text;
using Isopoh.Cryptography.Argon2;

namespace BLFramework.Services
{
    /// <summary>
    /// Static service for password hashing and verification using double hashing.
    /// Implements a two-layer security approach:
    /// - Frontend: SHA512 hash of password
    /// - Backend: Argon2id hash of the SHA512 hash
    /// This prevents password plaintext exposure even during transit.
    /// </summary>
    public static class PasswordHashingService
    {
        /// <summary>
        /// Hashes a SHA512 password hash using Argon2id algorithm.
        /// The input should be the SHA512 hash received from the frontend.
        /// Argon2id provides strong protection against brute-force attacks.
        /// </summary>
        /// <param name="sha512Hash">The SHA512 hash of the password from the frontend.</param>
        /// <returns>The Argon2id hash string suitable for storage in the database.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the hashing operation fails.</exception>
        public static string HashPassword(string sha512Hash)
        {
            try
            {
                // Apply Argon2id hashing to the SHA512 hash
                // This creates a strong, salted hash resistant to brute-force attacks
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
        /// Uses constant-time comparison to prevent timing attacks.
        /// </summary>
        /// <param name="sha512Hash">The SHA512 hash of the password from the frontend.</param>
        /// <param name="argon2Hash">The stored Argon2id hash to verify against.</param>
        /// <returns>True if the hashes match, false if they don't or if verification fails.</returns>
        public static bool VerifyPassword(string sha512Hash, string argon2Hash)
        {
            try
            {
                // Verify using Argon2 static method - uses constant-time comparison
                // Returns true only if the SHA512 hash matches the Argon2id stored hash
                var verified = Argon2.Verify(argon2Hash, sha512Hash);
                return verified;
            }
            catch
            {
                // Return false on any error to prevent information leakage
                return false;
            }
        }
    }
}
