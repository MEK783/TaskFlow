using BLFramework.Data;
using BLFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BLFramework.Services
{
    /// <summary>
    /// Service for Invite entity operations.
    /// Manages user invitation code lifecycle including generation, validation, usage, and revocation.
    /// Ensures controlled user registration through invitation-only access.
    /// </summary>
    public class InviteService : BaseService<Invite>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InviteService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger for service operations.</param>
        public InviteService(AppDbContext context, ILogger<BaseService<Invite>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves an invite by its invitation code asynchronously.
        /// Eager loads the creator and user who used the invite.
        /// </summary>
        /// <param name="inviteCode">The invitation code to search for.</param>
        /// <returns>The Invite if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown if inviteCode is null or empty.</exception>
        public async Task<Invite?> GetByCodeAsync(string inviteCode)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(inviteCode))
                {
                    throw new ArgumentException("InviteCode cannot be null or empty", nameof(inviteCode));
                }

                // Query by code and include related user entities
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
        /// Retrieves all invites created by a specific user asynchronously.
        /// Results include who used each invite and are ordered by creation date (newest first).
        /// </summary>
        /// <param name="userId">The ID of the user who created the invites.</param>
        /// <returns>A list of invites created by the specified user, ordered by creation date descending.</returns>
        public async Task<List<Invite>> GetInvitesByCreatorAsync(int userId)
        {
            try
            {
                // Get all invites created by the user, ordered by creation date descending
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
        /// Retrieves all valid (not expired and not used) invitations asynchronously.
        /// </summary>
        /// <returns>A list of all invites that can still be used for registration.</returns>
        public async Task<List<Invite>> GetValidInvitesAsync()
        {
            try
            {
                // Get invites that haven't expired and haven't been used
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
        /// Retrieves all expired invitations asynchronously.
        /// </summary>
        /// <returns>A list of all invites that have passed their expiration time.</returns>
        public async Task<List<Invite>> GetExpiredInvitesAsync()
        {
            try
            {
                // Get invites where ExpiresOn is in the past
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
        /// Retrieves all used invitations asynchronously.
        /// </summary>
        /// <returns>A list of all invites that have been used for user registration.</returns>
        public async Task<List<Invite>> GetUsedInvitesAsync()
        {
            try
            {
                // Get invites where UsedOn is not null, include creator and user who used it
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
        /// Checks if an invitation code already exists asynchronously.
        /// </summary>
        /// <param name="inviteCode">The invitation code to check.</param>
        /// <returns>True if the code exists; otherwise, false.</returns>
        public async Task<bool> InviteCodeExistsAsync(string inviteCode)
        {
            try
            {
                // Return false for null/empty input
                if (string.IsNullOrWhiteSpace(inviteCode))
                {
                    return false;
                }

                // Check if any invite has this code
                return await _context.Invites.AnyAsync(i => i.InviteCode == inviteCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if invite code exists {InviteCode}", inviteCode);
                throw;
            }
        }

        /// <summary>
        /// Marks an invitation as used asynchronously.
        /// Associates the invite with the user who used it and sets the usage timestamp.
        /// </summary>
        /// <param name="inviteCode">The invitation code being used.</param>
        /// <param name="usedById">The ID of the user using the invitation.</param>
        /// <returns>The updated invite.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the invite is not found.</exception>
        /// <exception cref="ValidationException">Thrown if the invite is invalid, expired, or already used.</exception>
        public async Task<Invite> UseInviteAsync(string inviteCode, int usedById)
        {
            try
            {
                // Retrieve the invite
                var invite = await GetByCodeAsync(inviteCode);
                if (invite == null)
                {
                    throw new KeyNotFoundException($"Invite with code {inviteCode} not found");
                }

                // Check if the invite is valid
                if (!invite.IsValid)
                {
                    // Provide specific error message for the reason it's invalid
                    if (invite.IsExpired)
                    {
                        throw new ValidationException("This invite has expired");
                    }
                    if (invite.IsUsed)
                    {
                        throw new ValidationException("This invite has already been used");
                    }
                }

                // Verify the user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == usedById);
                if (!userExists)
                {
                    throw new ValidationException($"User with ID {usedById} does not exist");
                }

                // Mark the invite as used
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
        /// Revokes an invitation by setting its expiration to the current time asynchronously.
        /// Prevents further use of the invitation.
        /// </summary>
        /// <param name="inviteId">The ID of the invite to revoke.</param>
        /// <returns>The revoked invite.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the invite is not found.</exception>
        /// <exception cref="ValidationException">Thrown if the invite has already been used.</exception>
        public async Task<Invite> RevokeInviteAsync(int inviteId)
        {
            try
            {
                // Retrieve the invite
                var invite = await GetByIdAsync(inviteId);
                if (invite == null)
                {
                    throw new KeyNotFoundException($"Invite with ID {inviteId} not found");
                }

                // Cannot revoke an already-used invite
                if (invite.IsUsed)
                {
                    throw new ValidationException("Cannot revoke an invite that has already been used");
                }

                // Set ExpiresOn to current time to invalidate the invite
                invite.ExpiresOn = DateTime.UtcNow;

                return await UpdateAsync(invite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking invite {InviteId}", inviteId);
                throw;
            }
        }

        /// <summary>
        /// Overrides base AddAsync to validate foreign keys and auto-generate unique invite codes.
        /// </summary>
        /// <param name="entity">The invite entity to add.</param>
        /// <returns>The created invite.</returns>
        /// <exception cref="ValidationException">Thrown if validation fails.</exception>
        public override async Task<Invite> AddAsync(Invite entity)
        {
            try
            {
                // Validate that the creator exists
                var creatorExists = await _context.Users.AnyAsync(u => u.Id == entity.CreatedById);
                if (!creatorExists)
                {
                    throw new ValidationException($"User with ID {entity.CreatedById} does not exist");
                }

                // Generate a unique invite code if not provided
                if (string.IsNullOrWhiteSpace(entity.InviteCode))
                {
                    entity.InviteCode = await GenerateUniqueInviteCodeAsync();
                }
                else
                {
                    // If code is provided, validate uniqueness
                    if (await InviteCodeExistsAsync(entity.InviteCode))
                    {
                        throw new ValidationException($"An invite with code '{entity.InviteCode}' already exists");
                    }
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
                _logger.LogError(ex, "Error adding invite");
                throw;
            }
        }

        /// <summary>
        /// Generates a unique 16-character invitation code using cryptographic randomness.
        /// Retries up to 10 times if collisions are detected.
        /// </summary>
        /// <returns>A unique invitation code not present in the database.</returns>
        /// <exception cref="InvalidOperationException">Thrown if unable to generate unique code after maximum attempts.</exception>
        private async Task<string> GenerateUniqueInviteCodeAsync()
        {
            const int maxAttempts = 10;
            int attempts = 0;

            // Retry logic to handle rare collision cases
            while (attempts < maxAttempts)
            {
                // Generate a new random code
                string code = Invite.GenerateInviteCode();
                // Check if this code already exists
                if (!await InviteCodeExistsAsync(code))
                {
                    return code;
                }
                attempts++;
            }

            throw new InvalidOperationException("Failed to generate a unique invite code after multiple attempts");
        }

        /// <summary>
        /// Overrides base UpdateAsync to set the UpdatedAt timestamp.
        /// </summary>
        /// <param name="entity">The invite entity with updated values.</param>
        /// <returns>The updated invite.</returns>
        public override async Task<Invite> UpdateAsync(Invite entity)
        {
            try
            {
                // Update the modification timestamp
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
