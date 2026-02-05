using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for RefreshToken entity operations
    /// </summary>
    public class RefreshTokenService : BaseService<RefreshToken>
    {
        public RefreshTokenService(AppDbContext context, ILogger<BaseService<RefreshToken>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Gets a refresh token by token string
        /// </summary>
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentException("Token cannot be null or empty", nameof(token));
                }

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
        /// Gets all active refresh tokens for a user
        /// </summary>
        public async Task<List<RefreshToken>> GetActiveTokensForUserAsync(int userId)
        {
            try
            {
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
        /// Gets all refresh tokens for a user (including expired and revoked)
        /// </summary>
        public async Task<List<RefreshToken>> GetTokensForUserAsync(int userId)
        {
            try
            {
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
        /// Gets all expired refresh tokens
        /// </summary>
        public async Task<List<RefreshToken>> GetExpiredTokensAsync()
        {
            try
            {
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
        /// Gets all revoked refresh tokens
        /// </summary>
        public async Task<List<RefreshToken>> GetRevokedTokensAsync()
        {
            try
            {
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
        /// Checks if a token string already exists
        /// </summary>
        public async Task<bool> TokenExistsAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                return await _context.RefreshTokens.AnyAsync(rt => rt.Token == token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if token exists");
                throw;
            }
        }

        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        public async Task<RefreshToken> RevokeTokenAsync(int tokenId)
        {
            try
            {
                var token = await GetByIdAsync(tokenId);
                if (token == null)
                {
                    throw new KeyNotFoundException($"RefreshToken with ID {tokenId} not found");
                }

                if (token.IsRevoked)
                {
                    throw new ValidationException("This token has already been revoked");
                }

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
        /// Revokes a refresh token by token string
        /// </summary>
        public async Task<RefreshToken> RevokeTokenByStringAsync(string token)
        {
            try
            {
                var refreshToken = await GetByTokenAsync(token);
                if (refreshToken == null)
                {
                    throw new KeyNotFoundException("RefreshToken not found");
                }

                return await RevokeTokenAsync(refreshToken.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token by string");
                throw;
            }
        }

        /// <summary>
        /// Revokes all tokens for a user
        /// </summary>
        public async Task RevokeAllTokensForUserAsync(int userId)
        {
            try
            {
                var tokens = await GetTokensForUserAsync(userId);
                var activeTokens = tokens.Where(t => !t.IsRevoked && !t.IsExpired).ToList();

                foreach (var token in activeTokens)
                {
                    token.RevokedOn = DateTime.UtcNow;
                }

                // Update multiple tokens
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
        /// Replaces an old token with a new one (revokes old, creates new)
        /// </summary>
        public async Task<RefreshToken> ReplaceTokenAsync(int oldTokenId, string newToken, DateTime newExpiresOn)
        {
            try
            {
                var oldToken = await GetByIdAsync(oldTokenId);
                if (oldToken == null)
                {
                    throw new KeyNotFoundException($"RefreshToken with ID {oldTokenId} not found");
                }

                // Check new token doesn't already exist
                if (await TokenExistsAsync(newToken))
                {
                    throw new ValidationException("The new token already exists");
                }

                // Set replacing token reference and revoke
                oldToken.ReplacingToken = newToken;
                oldToken.RevokedOn = DateTime.UtcNow;
                await UpdateAsync(oldToken);

                // Create new token
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
        /// Deletes old expired tokens (cleanup operation)
        /// </summary>
        public async System.Threading.Tasks.Task<int> DeleteExpiredTokensAsync(int daysOld = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                var tokensToDelete = await _context.RefreshTokens
                    .Where(rt => rt.ExpiresOn < cutoffDate && rt.RevokedOn != null)
                    .ToListAsync();

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

        public override async Task<RefreshToken> AddAsync(RefreshToken entity)
        {
            try
            {
                // Validate foreign key
                var userExists = await _context.Users.AnyAsync(u => u.Id == entity.UserId);
                if (!userExists)
                {
                    throw new ValidationException($"User with ID {entity.UserId} does not exist");
                }

                // Validate unique constraint: Token
                if (await TokenExistsAsync(entity.Token))
                {
                    throw new ValidationException("This token already exists");
                }

                // Validate ExpiresOn is in the future
                if (entity.ExpiresOn <= DateTime.UtcNow)
                {
                    throw new ValidationException("ExpiresOn must be in the future");
                }

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

        public override async Task<RefreshToken> UpdateAsync(RefreshToken entity)
        {
            try
            {
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
