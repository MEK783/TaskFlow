using BLFramework.Models;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for handling user authentication operations.
    /// Manages user registration, login, and password verification.
    /// </summary>
    public class AuthenticationService
    {
        private readonly UserService _userService;
        private readonly InviteService _inviteService;

        public AuthenticationService(UserService userService, InviteService inviteService)
        {
            _userService = userService;
            _inviteService = inviteService;
        }

        /// <summary>
        /// Registers a new user with an invite code.
        /// </summary>
        /// <param name="username">The username for the new user</param>
        /// <param name="sha512PasswordHash">The SHA512 hash of the password from frontend</param>
        /// <param name="inviteCode">The invite code for registration</param>
        /// <returns>The created User object</returns>
        /// <exception cref="InvalidOperationException">Thrown when invite is invalid or username exists</exception>
        public async Task<User> RegisterUserAsync(string username, string sha512PasswordHash, string inviteCode)
        {
            // Validate invite code
            var invite = await _inviteService.GetByCodeAsync(inviteCode);
            if (invite == null || !invite.IsValid)
            {
                throw new InvalidOperationException("Invalid or expired invite code");
            }

            // Check if username already exists
            if (await _userService.UsernameExistsAsync(username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            // Hash the SHA512 password using Argon2id
            var argon2Hash = PasswordHashingService.HashPassword(sha512PasswordHash);

            // Create new user
            var newUser = new User
            {
                Username = username,
                Password = argon2Hash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            var createdUser = await _userService.AddAsync(newUser);

            // Mark invite as used
            await _inviteService.UseInviteAsync(inviteCode, createdUser.Id);

            return createdUser;
        }

        /// <summary>
        /// Authenticates a user with their username and password hash.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="sha512PasswordHash">The SHA512 hash of the password from frontend</param>
        /// <returns>The authenticated User object or null if authentication fails</returns>
        public async Task<User?> AuthenticateUserAsync(string username, string sha512PasswordHash)
        {
            // Get user by username
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return null;
            }

            // Verify password
            if (!PasswordHashingService.VerifyPassword(sha512PasswordHash, user.Password))
            {
                return null;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return null;
            }

            return user;
        }

        /// <summary>
        /// Verifies if a password hash matches the stored hash for a user.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="sha512PasswordHash">The SHA512 hash of the password from frontend</param>
        /// <returns>True if password matches, false otherwise</returns>
        public async Task<bool> VerifyUserPasswordAsync(string username, string sha512PasswordHash)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return false;
            }

            return PasswordHashingService.VerifyPassword(sha512PasswordHash, user.Password);
        }
    }
}
