using BLFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace BLFramework.Data
{
    /// <summary>
    /// Application DbContext for Azure SQL Database integration.
    /// Manages entity mappings, relationships, indexes, and database schema configuration.
    /// Configured with retry resilience for transient network failures.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for configuring the DbContext.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the User entity set.
        /// Contains all registered application users.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the TaskStatusDefinition entity set.
        /// Contains all available task status definitions used by the application.
        /// </summary>
        public DbSet<TaskStatusDefinition> TaskStatusDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the UserTask entity set.
        /// Contains all user-created tasks.
        /// </summary>
        public DbSet<UserTask> Tasks { get; set; }

        /// <summary>
        /// Gets or sets the Invite entity set.
        /// Contains all user invitations for controlled registration access.
        /// </summary>
        public DbSet<Invite> Invites { get; set; }

        /// <summary>
        /// Gets or sets the RefreshToken entity set.
        /// Contains all JWT refresh tokens issued to users.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Configures the entity mappings, relationships, constraints, and indexes.
        /// Called during DbContext initialization and applied to all database operations.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                // Map to the Users table
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                // Configure Username property
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(32);

                // Configure Password property - stores Argon2id hash
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(200);

                // Configure IsActive property with default true value
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // Configure CreatedAt property mapped to CreatedOn column
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                // Configure LastLogin property with current date default
                entity.Property(e => e.LastLogin)
                    .HasDefaultValueSql("GETDATE()");

                // Create unique index on Username for efficient lookups and constraint enforcement
                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("UQ_Users_Username");

                // Configure relationships
                entity.HasMany(e => e.CreatedTasks)
                    .WithOne(t => t.CreatedBy)
                    .HasForeignKey(t => t.CreatedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.CreatedInvites)
                    .WithOne(i => i.CreatedBy)
                    .HasForeignKey(i => i.CreatedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.UsedInvites)
                    .WithOne(i => i.UsedBy)
                    .HasForeignKey(i => i.UsedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure TaskStatusDefinition entity
            modelBuilder.Entity<TaskStatusDefinition>(entity =>
            {
                // Map to the TaskStatus table
                entity.ToTable("TaskStatus");
                entity.HasKey(e => e.Id);

                // Configure StatusCode property - internal status identifier
                entity.Property(e => e.StatusCode)
                    .IsRequired()
                    .HasMaxLength(50);

                // Configure StatusDescription property - user-facing status description
                entity.Property(e => e.StatusDescription)
                    .HasMaxLength(200);

                // Configure ReactIcon property - React icon identifier for UI display
                entity.Property(e => e.ReactIcon)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("");

                // Configure ClosingStatus property - indicates if status closes the task
                entity.Property(e => e.ClosingStatus)
                    .HasDefaultValue(false);

                // Set CreatedAt to be generated on add
                entity.Property(e => e.CreatedAt)
                    .ValueGeneratedOnAdd();

                // Configure relationship with tasks
                entity.HasMany(e => e.Tasks)
                    .WithOne(t => t.Status)
                    .HasForeignKey(t => t.StatusId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure UserTask entity
            modelBuilder.Entity<UserTask>(entity =>
            {
                // Map to the Tasks table
                entity.ToTable("Tasks");
                entity.HasKey(e => e.Id);

                // Configure TaskName property - user-provided task title
                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValue("");

                // Configure TaskDescription property - allows detailed task information
                entity.Property(e => e.TaskDescription)
                    .HasColumnType("NVARCHAR(MAX)");

                // Configure StatusId foreign key column
                entity.Property(e => e.StatusId)
                    .HasColumnName("Status_TaskStatusFK");

                // Configure CreatedById foreign key column
                entity.Property(e => e.CreatedById)
                    .HasColumnName("CreatedBy_UserFK");

                // Configure StatusPriority property - determines task order within status
                entity.Property(e => e.StatusPriority)
                    .HasDefaultValue(0);

                // Configure CreatedAt property mapped to CreatedOn column
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                // Configure ModifiedOn property - updated on task modifications
                entity.Property(e => e.ModifiedOn)
                    .HasDefaultValueSql("GETDATE()");

                // Configure ClosedOn property - nullable timestamp for when task was closed
                entity.Property(e => e.ClosedOn)
                    .IsRequired(false);

                // Create unique index to ensure task names are unique per user
                entity.HasIndex(e => new { e.TaskName, e.CreatedById })
                    .IsUnique()
                    .HasDatabaseName("UQ_Tasks_TaskName");

                // Create unique index to enforce priority uniqueness within user/status combination
                entity.HasIndex(e => new { e.StatusId, e.CreatedById, e.StatusPriority })
                    .IsUnique()
                    .HasDatabaseName("UQ_Tasks_StatusPriority");

                // Create non-unique index for status/user/priority queries
                entity.HasIndex(e => new { e.StatusId, e.CreatedById, e.StatusPriority })
                    .HasDatabaseName("CK_Tasks_StatusPriority");

                // Configure foreign key relationships
                entity.HasOne(e => e.Status)
                    .WithMany(ts => ts.Tasks)
                    .HasForeignKey(e => e.StatusId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.CreatedTasks)
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Invite entity
            modelBuilder.Entity<Invite>(entity =>
            {
                // Map to the Invites table
                entity.ToTable("Invites");
                entity.HasKey(e => e.Id);

                // Configure InviteCode property - 16-character unique code
                entity.Property(e => e.InviteCode)
                    .IsRequired()
                    .HasMaxLength(16);

                // Configure CreatedById foreign key column
                entity.Property(e => e.CreatedById)
                    .HasColumnName("CreatedBy_UserFK");

                // Configure UsedById foreign key column - nullable until used
                entity.Property(e => e.UsedById)
                    .HasColumnName("UsedBy_UserFK");

                // Configure CreatedAt property mapped to CreatedOn column
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                // Configure ExpiresOn property - defaults to 15 days from creation
                entity.Property(e => e.ExpiresOn)
                    .HasDefaultValueSql("DATEADD(DD, 15, GETDATE())");

                // Create unique index to ensure no duplicate invite codes
                entity.HasIndex(e => e.InviteCode)
                    .IsUnique()
                    .HasDatabaseName("UQ_Invites_InviteCode");

                // Configure foreign key relationships
                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.CreatedInvites)
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.UsedBy)
                    .WithMany(u => u.UsedInvites)
                    .HasForeignKey(e => e.UsedById)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                // Map to the RefreshTokens table
                entity.ToTable("RefreshTokens");
                entity.HasKey(e => e.Id);

                // Configure Token property - 20-128 character token string
                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(128);

                // Configure UserId foreign key column
                entity.Property(e => e.UserId)
                    .HasColumnName("For_UsersFK");

                // Configure CreatedAt property mapped to CreatedOn column
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                // Configure ReplacingToken property - stores new token if refreshed
                entity.Property(e => e.ReplacingToken)
                    .HasMaxLength(128);

                // Create unique index to ensure token uniqueness
                entity.HasIndex(e => e.Token)
                    .IsUnique()
                    .HasDatabaseName("UQ_RefreshTokens_Token");

                // Create filtered unique index for active (non-revoked) tokens per user
                entity.HasIndex(e => new { e.UserId, e.Token })
                    .IsUnique()
                    .HasDatabaseName("UX_RefreshTokens_ActiveTokens")
                    .HasFilter("[RevokedOn] IS NULL");

                // Configure foreign key relationship
                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
