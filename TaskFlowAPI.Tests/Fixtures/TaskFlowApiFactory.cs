using System.Collections.Generic;
using BLFramework.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskFlowAPI.Services;

namespace TaskFlowAPI.Tests.Fixtures
{
    public class TaskFlowApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"TaskFlowApiTests-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "InMemory",
                    ["ClosedTaskCleanup:Enabled"] = "false",
                    ["ClosedTaskCleanup:IntervalDays"] = "365",
                    ["ClosedTaskCleanup:DeleteAfterDays"] = "365",
                    ["ClosedTaskCleanup:BatchSize"] = "1"
                };

                config.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                var hostedServices = services
                    .Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(ClosedTaskCleanupService))
                    .ToList();

                foreach (var hostedService in hostedServices)
                {
                    services.Remove(hostedService);
                }

                var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            });
        }

        public async Task SeedDefaultDataAsync()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await TestDataSeeder.ResetAsync(context);
            await TestDataSeeder.SeedAsync(context);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
        }
    }
}
