using BLFramework.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLFramework.Services
{
    /// <summary>
    /// Generic repository service providing common database operations.
    /// Implements CRUD operations and error logging for any entity type.
    /// </summary>
    /// <typeparam name="T">The entity type for database operations.</typeparam>
    public class BaseService<T> where T : class
    {
        /// <summary>
        /// The application database context.
        /// </summary>
        protected readonly AppDbContext _context;

        /// <summary>
        /// The logger instance for this service.
        /// </summary>
        protected readonly ILogger<BaseService<T>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseService{T}"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="logger">The logger for service operations.</param>
        public BaseService(AppDbContext context, ILogger<BaseService<T>> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all entities of type <typeparamref name="T"/> asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of all entities.</returns>
        /// <exception cref="Exception">Logged and rethrown if the database operation fails.</exception>
        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                return await _context.Set<T>().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Retrieves an entity by its ID asynchronously.
        /// </summary>
        /// <param name="id">The primary key value of the entity to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        /// <exception cref="Exception">Logged and rethrown if the database operation fails.</exception>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// Adds a new entity to the database asynchronously.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the added entity.</returns>
        /// <exception cref="Exception">Logged and rethrown if the database operation fails.</exception>
        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                // Add entity to the DbSet and save changes to persist to database
                await _context.Set<T>().AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing entity in the database asynchronously.
        /// </summary>
        /// <param name="entity">The entity with updated values.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the updated entity.</returns>
        /// <exception cref="Exception">Logged and rethrown if the database operation fails.</exception>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                var entry = _context.Entry(entity);
                // Only call Update if not already tracked by the context
                if (entry.State == EntityState.Detached)
                {
                    _context.Set<T>().Update(entity);
                }
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Deletes an entity from the database asynchronously.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Logged and rethrown if the database operation fails.</exception>
        public virtual async Task DeleteAsync(T entity)
        {
            try
            {
                // Remove entity from DbSet and persist deletion to database
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }
    }
}
