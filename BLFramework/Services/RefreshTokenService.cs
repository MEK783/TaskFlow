using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for RefreshToken entity operations.
    /// Manages JWT refresh token lifecycle including creation, validation, revocation, and replacement.
    /// Handles token cleanup and user logout by revoking all tokens.
    /// </summary>
    public class RefreshTokenService : BaseService<RefreshToken>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger for service operations.</param>
        public RefreshTokenService(AppDbContext context, ILogger<BaseService<RefreshToken>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves a refresh token by its token string asynchronously.
        /// Eager loads the related User navigation property.
        /// </summary>
        /// <param name="token">The token string to search for.</param>
        /// <returns>The RefreshToken if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown if token is null or empty.</exception>
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentException("Token cannot be null or empty", nameof(token));
                }

                // Query by token string and include the related user
                return await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refresh token by token");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all active (non-expired and non-revoked) refresh tokens for a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of active refresh tokens for the specified user.</returns>
        public async Task<List<RefreshToken>> GetActiveTokensForUserAsync(int userId)
        {
            try
            {
                // Get tokens that are not expired and not revoked
                return await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.ExpiresOn > DateTime.UtcNow && rt.RevokedOn == null)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active refresh tokens for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all refresh tokens for a user asynchronously, including expired and revoked tokens.
        /// Results are ordered by creation date in descending order (newest first).
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of all refresh tokens for the specified user.</returns>
        public async Task<List<RefreshToken>> GetTokensForUserAsync(int userId)
        {
            try
            {
                // Get all tokens for the user, ordered by creation date descending
                return await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId)
                    .OrderByDescending(rt => rt.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all refresh tokens for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all expired refresh tokens asynchronously.
        /// Useful for cleanup operations to remove obsolete tokens from the database.
        /// </summary>
        /// <returns>A list of all expired refresh tokens.</returns>
        public async Task<List<RefreshToken>> GetExpiredTokensAsync()
        {
            try
            {
                // Get tokens where ExpiresOn is in the past
                return await _context.RefreshTokens
                    .Where(rt => rt.ExpiresOn <= DateTime.UtcNow)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expired refresh tokens");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all revoked refresh tokens asynchronously.
        /// </summary>
        /// <returns>A list of all revoked refresh tokens.</returns>
        public async Task<List<RefreshToken>> GetRevokedTokensAsync()
        {
            try
            {
                // Get tokens where RevokedOn has been set
                return await _context.RefreshTokens
                    .Where(rt => rt.RevokedOn != null)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revoked refresh tokens");
                throw;
            }
        }

        /// <summary>
        /// Checks if a token string already exists asynchronously.
        /// Used to enforce token uniqueness before creating a new token.
        /// Only considers non-expired, non-revoked tokens as existing.
        /// </summary>
        /// <param name="token">The token string to check.</param>
        /// <returns>True if the token exists and is valid (not expired/revoked); otherwise, false.</returns>
        public async Task<bool> TokenExistsAsync(string token)
        {
            try
            {
                // Return false for null/empty input
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                // Check if any token has this string value and is not expired or revoked
                return await _context.RefreshTokens.AnyAsync(rt => 
                    rt.Token == token && 
                    rt.ExpiresOn > DateTime.UtcNow && 
                    rt.RevokedOn == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if token exists");
                throw;
            }
        }

        /// <summary>
        /// Revokes a refresh token by ID asynchronously.
        /// Prevents further use of the token for obtaining new access tokens.
        /// </summary>
        /// <param name="tokenId">The ID of the token to revoke.</param>
        /// <returns>The revoked token.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the token is not found.</exception>
        /// <exception cref="ValidationException">Thrown if the token has already been revoked.</exception>
        public async Task<RefreshToken> RevokeTokenAsync(int tokenId)
        {
            try
            {
                // Retrieve the token
                var token = await GetByIdAsync(tokenId);
                if (token == null)
                {
                    throw new KeyNotFoundException($"RefreshToken with ID {tokenId} not found");
                }

                // Prevent revoking an already-revoked token
                if (token.IsRevoked)
                {
                    throw new ValidationException("This token has already been revoked");
                }

                // Set RevokedOn to current UTC time
                token.RevokedOn = DateTime.UtcNow;

                return await UpdateAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token {TokenId}", tokenId);
                throw;
            }
        }

        /// <summary>
        /// Revokes a refresh token by its token string asynchronously.
        /// </summary>
        /// <param name="token">The token string to revoke.</param>
        /// <returns>The revoked token.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the token is not found.</exception>
        public async Task<RefreshToken> RevokeTokenByStringAsync(string token)
        {
            try
            {
                // Get the token by its string value
                var refreshToken = await GetByTokenAsync(token);
                if (refreshToken == null)
                {
                    throw new KeyNotFoundException("RefreshToken not found");
                }

                // Revoke using the ID
                return await RevokeTokenAsync(refreshToken.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token by string");
                throw;
            }
        }

        /// <summary>
        /// Revokes all active tokens for a user asynchronously.
        /// Used for logout functionality to invalidate all sessions.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        public async Task RevokeAllTokensForUserAsync(int userId)
        {
            try
            {
                // Get all tokens for the user
                var tokens = await GetTokensForUserAsync(userId);
                // Filter to only active (non-revoked, non-expired) tokens
                var activeTokens = tokens.Where(t => !t.IsRevoked && !t.IsExpired).ToList();

                // Revoke all active tokens
                foreach (var token in activeTokens)
                {
                    token.RevokedOn = DateTime.UtcNow;
                }

                // Batch update all tokens and save changes
                _context.RefreshTokens.UpdateRange(activeTokens);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Replaces an old refresh token with a new one asynchronously.
        /// Revokes the old token and creates a new token, maintaining the user association.
        /// </summary>
        /// <param name="oldTokenId">The ID of the token to replace.</param>
        /// <param name="newToken">The new token string.</param>
        /// <param name="newExpiresOn">The expiration date/time for the new token.</param>
        /// <returns>The newly created refresh token.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the old token is not found.</exception>
        /// <exception cref="ValidationException">Thrown if the new token already exists.</exception>
        public async Task<RefreshToken> ReplaceTokenAsync(int oldTokenId, string newToken, DateTime newExpiresOn)
        {
            try
            {
                // Retrieve the old token
                var oldToken = await GetByIdAsync(oldTokenId);
                if (oldToken == null)
                {
                    throw new KeyNotFoundException($"RefreshToken with ID {oldTokenId} not found");
                }

                // Validate that the new token doesn't already exist
                if (await TokenExistsAsync(newToken))
                {
                    throw new ValidationException("The new token already exists");
                }

                // Set the ReplacingToken reference and revoke the old token
                oldToken.ReplacingToken = newToken;
                oldToken.RevokedOn = DateTime.UtcNow;
                await UpdateAsync(oldToken);

                // Create a new refresh token for the same user
                var refreshToken = new RefreshToken
                {
                    Token = newToken,
                    UserId = oldToken.UserId,
                    ExpiresOn = newExpiresOn,
                    CreatedAt = DateTime.UtcNow
                };

                return await base.AddAsync(refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing token {TokenId}", oldTokenId);
                throw;
            }
        }

        /// <summary>
        /// Deletes old expired and revoked tokens asynchronously.
        /// Cleanup operation to remove obsolete tokens from the database.
        /// </summary>
        /// <param name="daysOld">Number of days old to consider for deletion (default: 30).</param>
        /// <returns>The number of tokens deleted.</returns>
        public async System.Threading.Tasks.Task<int> DeleteExpiredTokensAsync(int daysOld = 30)
        {
            try
            {
                // Calculate cutoff date (default 30 days in the past)
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                // Find revoked tokens that expired before the cutoff date
                var tokensToDelete = await _context.RefreshTokens
                    .Where(rt => rt.ExpiresOn < cutoffDate && rt.RevokedOn != null)
                    .ToListAsync();

                // Delete tokens if any exist
                if (tokensToDelete.Count > 0)
                {
                    _context.RefreshTokens.RemoveRange(tokensToDelete);
                    await _context.SaveChangesAsync();
                }

                return tokensToDelete.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expired tokens");
                throw;
            }
        }

        /// <summary>
        /// Overrides base AddAsync to validate user existence and token uniqueness.
        /// </summary>
        /// <param name="entity">The refresh token entity to add.</param>
        /// <returns>The created refresh token.</returns>
        /// <exception cref="ValidationException">Thrown if validation fails.</exception>
        public override async Task<RefreshToken> AddAsync(RefreshToken entity)
        {
            try
            {
                // Validate that the user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == entity.UserId);
                if (!userExists)
                {
                    throw new ValidationException($"User with ID {entity.UserId} does not exist");
                }

                // Validate unique constraint on Token
                if (await TokenExistsAsync(entity.Token))
                {
                    throw new ValidationException("This token already exists");
                }

                // Validate that the expiration is in the future
                if (entity.ExpiresOn <= DateTime.UtcNow)
                {
                    throw new ValidationException("ExpiresOn must be in the future");
                }

                // Set audit fields
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = null;

                return await base.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding refresh token");
                throw;
            }
        }

        /// <summary>
        /// Overrides base UpdateAsync to set the UpdatedAt timestamp.
        /// </summary>
        /// <param name="entity">The refresh token entity with updated values.</param>
        /// <returns>The updated refresh token.</returns>
        public override async Task<RefreshToken> UpdateAsync(RefreshToken entity)
        {
            try
            {
                // Update the modification timestamp
                entity.UpdatedAt = DateTime.UtcNow;
                return await base.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refresh token");
                throw;
            }
        }
    }
}
