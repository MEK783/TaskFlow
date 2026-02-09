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
                    // Enable retry on failure for transient faults in Azure SQL due to database pausing
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10, // Maximum number of retry attempts
                        maxRetryDelay: TimeSpan.FromSeconds(30), // Maximum delay between retries
                        errorNumbersToAdd: new[] { 4060, 10928, 10929, 40197, 40501, 40613 } // SQL error codes for transient faults in Azure SQL
                        );
                    sqlOptions.CommandTimeout(90);
                }));

            return services;
        }
    }
}
