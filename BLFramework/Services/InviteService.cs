using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for Invite entity operations
    /// </summary>
    public class InviteService : BaseService<Invite>
    {
        public InviteService(AppDbContext context, ILogger<BaseService<Invite>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Gets an invite by invite code
        /// </summary>
        public async Task<Invite?> GetByCodeAsync(string inviteCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(inviteCode))
                {
                    throw new ArgumentException("InviteCode cannot be null or empty", nameof(inviteCode));
                }

                return await _context.Invites
                    .Include(i => i.CreatedBy)
                    .Include(i => i.UsedBy)
                    .FirstOrDefaultAsync(i => i.InviteCode == inviteCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invite by code {InviteCode}", inviteCode);
                throw;
            }
        }

        /// <summary>
        /// Gets all invites created by a specific user with details
        /// </summary>
        public async Task<List<Invite>> GetInvitesByCreatorAsync(int userId)
        {
            try
            {
                return await _context.Invites
                    .Where(i => i.CreatedById == userId)
                    .Include(i => i.UsedBy)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invites created by user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets all valid (not expired and not used) invites
        /// </summary>
        public async Task<List<Invite>> GetValidInvitesAsync()
        {
            try
            {
                return await _context.Invites
                    .Where(i => i.ExpiresOn > DateTime.UtcNow && i.UsedOn == null)
                    .Include(i => i.CreatedBy)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving valid invites");
                throw;
            }
        }

        /// <summary>
        /// Gets all expired invites
        /// </summary>
        public async Task<List<Invite>> GetExpiredInvitesAsync()
        {
            try
            {
                return await _context.Invites
                    .Where(i => i.ExpiresOn <= DateTime.UtcNow)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expired invites");
                throw;
            }
        }

        /// <summary>
        /// Gets all used invites
        /// </summary>
        public async Task<List<Invite>> GetUsedInvitesAsync()
        {
            try
            {
                return await _context.Invites
                    .Where(i => i.UsedOn != null)
                    .Include(i => i.CreatedBy)
                    .Include(i => i.UsedBy)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving used invites");
                throw;
            }
        }

        /// <summary>
        /// Checks if an invite code already exists
        /// </summary>
        public async Task<bool> InviteCodeExistsAsync(string inviteCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(inviteCode))
                {
                    return false;
                }

                return await _context.Invites.AnyAsync(i => i.InviteCode == inviteCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if invite code exists {InviteCode}", inviteCode);
                throw;
            }
        }

        /// <summary>
        /// Uses an invite (marks it as used)
        /// </summary>
        public async Task<Invite> UseInviteAsync(string inviteCode, int usedById)
        {
            try
            {
                var invite = await GetByCodeAsync(inviteCode);
                if (invite == null)
                {
                    throw new KeyNotFoundException($"Invite with code {inviteCode} not found");
                }

                if (!invite.IsValid)
                {
                    if (invite.IsExpired)
                    {
                        throw new ValidationException("This invite has expired");
                    }
                    if (invite.IsUsed)
                    {
                        throw new ValidationException("This invite has already been used");
                    }
                }

                var userExists = await _context.Users.AnyAsync(u => u.Id == usedById);
                if (!userExists)
                {
                    throw new ValidationException($"User with ID {usedById} does not exist");
                }

                invite.UsedOn = DateTime.UtcNow;
                invite.UsedById = usedById;

                return await UpdateAsync(invite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error using invite {InviteCode}", inviteCode);
                throw;
            }
        }

        /// <summary>
        /// Revokes an invite (marks it as expired by setting ExpiresOn to now)
        /// </summary>
        public async Task<Invite> RevokeInviteAsync(int inviteId)
        {
            try
            {
                var invite = await GetByIdAsync(inviteId);
                if (invite == null)
                {
                    throw new KeyNotFoundException($"Invite with ID {inviteId} not found");
                }

                if (invite.IsUsed)
                {
                    throw new ValidationException("Cannot revoke an invite that has already been used");
                }

                invite.ExpiresOn = DateTime.UtcNow;

                return await UpdateAsync(invite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking invite {InviteId}", inviteId);
                throw;
            }
        }

        public override async Task<Invite> AddAsync(Invite entity)
        {
            try
            {
                // Validate foreign keys
                var creatorExists = await _context.Users.AnyAsync(u => u.Id == entity.CreatedById);
                if (!creatorExists)
                {
                    throw new ValidationException($"User with ID {entity.CreatedById} does not exist");
                }

                // Generate a unique invite code if not already set
                if (string.IsNullOrWhiteSpace(entity.InviteCode))
                {
                    entity.InviteCode = await GenerateUniqueInviteCodeAsync();
                }
                else
                {
                    // Validate unique constraint: InviteCode
                    if (await InviteCodeExistsAsync(entity.InviteCode))
                    {
                        throw new ValidationException($"An invite with code '{entity.InviteCode}' already exists");
                    }
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
                _logger.LogError(ex, "Error adding invite");
                throw;
            }
        }

        /// <summary>
        /// Generates a unique invite code that doesn't already exist in the database
        /// </summary>
        private async Task<string> GenerateUniqueInviteCodeAsync()
        {
            const int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                string code = Invite.GenerateInviteCode();
                if (!await InviteCodeExistsAsync(code))
                {
                    return code;
                }
                attempts++;
            }

            throw new InvalidOperationException("Failed to generate a unique invite code after multiple attempts");
        }

        public override async Task<Invite> UpdateAsync(Invite entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                return await base.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invite");
                throw;
            }
        }
    }
}
