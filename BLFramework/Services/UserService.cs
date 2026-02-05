using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for User entity operations
    /// </summary>
    public class UserService : BaseService<User>
    {
        public UserService(AppDbContext context, ILogger<BaseService<User>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Gets a user by username
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Username cannot be null or empty", nameof(username));
                }

                return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Gets all active users
        /// </summary>
        public async Task<List<User>> GetActiveUsersAsync()
        {
            try
            {
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
        /// Checks if a username already exists
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return false;
                }

                return await _context.Users.AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if username exists {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Updates the last login timestamp for a user
        /// </summary>
        public async Task<User> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

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
        /// Deactivates a user
        /// </summary>
        public async Task<User> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

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
        /// Reactivates a user
        /// </summary>
        public async Task<User> ReactivateUserAsync(int userId)
        {
            try
            {
                var user = await GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                user.IsActive = true;
                return await UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user {UserId}", userId);
                throw;
            }
        }

        public override async Task<User> AddAsync(User entity)
        {
            try
            {
                // Validate constraints
                if (await UsernameExistsAsync(entity.Username))
                {
                    throw new ValidationException($"Username '{entity.Username}' already exists");
                }

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

        public override async Task<User> UpdateAsync(User entity)
        {
            try
            {
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
