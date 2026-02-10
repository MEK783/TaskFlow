namespace BLFramework.Models
{
    /// <summary>
    /// Base entity with common properties for all domain models.
    /// Provides audit tracking with creation and update timestamps.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// Auto-incremented by the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// Defaults to UTC now and is set by the database.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the entity was last updated.
        /// Nullable; remains null if the entity has never been updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
