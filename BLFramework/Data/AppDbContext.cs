using BLFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace BLFramework.Data
{
    /// <summary>
    /// Application DbContext for Azure SQL Database integration
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskStatusDefinition> TaskStatusDefinitions { get; set; }
        public DbSet<UserTask> Tasks { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.LastLogin)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("UQ_Users_Username");

                // Relationships
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
                entity.ToTable("TaskStatus");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.StatusCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.StatusDescription)
                    .HasMaxLength(200);

                entity.Property(e => e.ReactIcon)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("");

                entity.Property(e => e.ClosingStatus)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedAt)
                    .ValueGeneratedOnAdd();

                entity.HasMany(e => e.Tasks)
                    .WithOne(t => t.Status)
                    .HasForeignKey(t => t.StatusId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure UserTask entity
            modelBuilder.Entity<UserTask>(entity =>
            {
                entity.ToTable("Tasks");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValue("");

                entity.Property(e => e.TaskDescription)
                    .HasColumnType("NVARCHAR(MAX)");

                entity.Property(e => e.StatusId)
                    .HasColumnName("Status_TaskStatusFK");

                entity.Property(e => e.CreatedById)
                    .HasColumnName("CreatedBy_UserFK");

                entity.Property(e => e.StatusPriority)
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.ModifiedOn)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => new { e.TaskName, e.CreatedById })
                    .IsUnique()
                    .HasDatabaseName("UQ_Tasks_TaskName");

                entity.HasIndex(e => new { e.StatusId, e.CreatedById, e.StatusPriority })
                    .IsUnique()
                    .HasDatabaseName("UQ_Tasks_StatusPriority");

                entity.HasIndex(e => new { e.StatusId, e.CreatedById, e.StatusPriority })
                    .HasDatabaseName("CK_Tasks_StatusPriority");

                // Foreign key relationships
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
                entity.ToTable("Invites");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.InviteCode)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.CreatedById)
                    .HasColumnName("CreatedBy_UserFK");

                entity.Property(e => e.UsedById)
                    .HasColumnName("UsedBy_UserFK");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.ExpiresOn)
                    .HasDefaultValueSql("DATEADD(DD, 15, GETDATE())");

                entity.HasIndex(e => e.InviteCode)
                    .IsUnique()
                    .HasDatabaseName("UQ_Invites_InviteCode");

                // Foreign key relationships
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
                entity.ToTable("RefreshTokens");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UserId)
                    .HasColumnName("For_UsersFK");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.ReplacingToken)
                    .HasMaxLength(128);

                entity.HasIndex(e => e.Token)
                    .IsUnique()
                    .HasDatabaseName("UQ_RefreshTokens_Token");

                entity.HasIndex(e => new { e.UserId, e.Token })
                    .IsUnique()
                    .HasDatabaseName("UX_RefreshTokens_ActiveTokens")
                    .HasFilter("[RevokedOn] IS NULL");

                // Foreign key relationship
                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
