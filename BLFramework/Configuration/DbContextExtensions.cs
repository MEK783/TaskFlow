using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BLFramework.Data;

namespace BLFramework.Configuration
{
    /// <summary>
    /// Extension methods for configuring database context
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Adds AppDbContext to the dependency injection container with Azure SQL configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="connectionString">Azure SQL connection string</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddAppDbContext(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
                    sqlOptions.CommandTimeout(30);
                }));

            return services;
        }
    }
}
