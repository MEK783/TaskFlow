using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for User entity operations.
    /// Handles user creation, retrieval, activation/deactivation, and validation.
    /// </summary>
    public class UserService : BaseService<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger for service operations.</param>
        public UserService(AppDbContext context, ILogger<BaseService<User>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves a user by username asynchronously.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The User if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown if username is null or empty.</exception>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Username cannot be null or empty", nameof(username));
                }

                // Query the database for the user with the matching username
                return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all active (non-deactivated) users asynchronously.
        /// </summary>
        /// <returns>A list of all users with IsActive set to true.</returns>
        public async Task<List<User>> GetActiveUsersAsync()
        {
            try
            {
                // Filter and return only active users
                return await _context.Users
                    .Where(u => u.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active users");
                throw;
            }
        }

        /// <summary>
        /// Checks if a username already exists in the database asynchronously.
        /// Used during registration to prevent duplicate usernames.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>True if the username exists; otherwise, false.</returns>
        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                // Return false for null/empty input
                if (string.IsNullOrWhiteSpace(username))
                {
                    return false;
                }

                // Check if any user has the specified username
                return await _context.Users.AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if username exists {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Updates the LastLogin timestamp for a user asynchronously.
        /// Called after successful authentication.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <returns>The updated User object.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user is not found.</exception>
        public async Task<User> UpdateLastLoginAsync(int userId)
        {
            try
            {
                // Retrieve user by ID
                var user = await GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                // Update the LastLogin timestamp to current UTC time
                user.LastLogin = DateTime.UtcNow;
                return await UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Deactivates a user account asynchronously.
        /// Deactivated users cannot authenticate or perform any operations.
        /// </summary>
        /// <param name="userId">The ID of the user to deactivate.</param>
        /// <returns>The deactivated User object.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user is not found.</exception>
        public async Task<User> DeactivateUserAsync(int userId)
        {
            try
            {
                // Retrieve user by ID
                var user = await GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                // Set IsActive to false
                user.IsActive = false;
                return await UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Reactivates a user account asynchronously.
        /// Allows a previously deactivated user to authenticate again.
        /// </summary>
        /// <param name="userId">The ID of the user to reactivate.</param>
        /// <returns>The reactivated User object.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user is not found.</exception>
        public async Task<User> ReactivateUserAsync(int userId)
        {
            try
            {
                // Retrieve user by ID
                var user = await GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                // Set IsActive to true
                user.IsActive = true;
                return await UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Overrides base AddAsync to validate username uniqueness and set audit fields.
        /// </summary>
        /// <param name="entity">The user entity to add.</param>
        /// <returns>The created User object.</returns>
        /// <exception cref="ValidationException">Thrown if username already exists.</exception>
        public override async Task<User> AddAsync(User entity)
        {
            try
            {
                // Validate that username doesn't already exist
                if (await UsernameExistsAsync(entity.Username))
                {
                    throw new ValidationException($"Username '{entity.Username}' already exists");
                }

                // Set audit fields: CreatedAt to now, UpdatedAt to null (never been updated)
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = null;
                return await base.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user");
                throw;
            }
        }

        /// <summary>
        /// Overrides base UpdateAsync to set the UpdatedAt timestamp.
        /// </summary>
        /// <param name="entity">The user entity with updated values.</param>
        /// <returns>The updated User object.</returns>
        public override async Task<User> UpdateAsync(User entity)
        {
            try
            {
                // Set UpdatedAt to current UTC time to track when the user was last modified
                entity.UpdatedAt = DateTime.UtcNow;
                return await base.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                throw;
            }
        }
    }
}
