using BLFramework.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLFramework.Services
{
    /// <summary>
    /// Generic repository service for database operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class BaseService<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly ILogger<BaseService<T>> _logger;

        public BaseService(AppDbContext context, ILogger<BaseService<T>> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all entities asynchronously
        /// </summary>
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
        /// Gets an entity by ID asynchronously
        /// </summary>
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
        /// Adds a new entity asynchronously
        /// </summary>
        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
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
        /// Updates an existing entity asynchronously
        /// </summary>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                _context.Set<T>().Update(entity);
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
        /// Deletes an entity asynchronously
        /// </summary>
        public virtual async Task DeleteAsync(T entity)
        {
            try
            {
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
