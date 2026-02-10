# BLFramework - Business Logic Layer with Entity Framework Core

A .NET 8.0 class library providing a business logic layer with Entity Framework Core integration for Azure SQL Database connectivity.

## Overview

BLFramework is designed as a reusable class library that encapsulates business logic and data access patterns using Entity Framework Core. It provides a configurable ADO.NET connection string for seamless Azure SQL Database connectivity.

## Features

- ✅ **Entity Framework Core 8.0** - Modern ORM for .NET
- ✅ **Azure SQL Database Support** - Full SQL Server compatibility
- ✅ **Dependency Injection Ready** - Configured for DI containers
- ✅ **Generic Repository Pattern** - BaseService with CRUD operations
- ✅ **Async/Await Support** - All database operations are asynchronous
- ✅ **Logging Integration** - Built-in logging for debugging
- ✅ **Retry Policy** - Automatic retry on transient failures
- ✅ **Configuration Management** - Externalized connection strings via appsettings.json

## Technology Stack

- **.NET Framework:** .NET 8.0
- **ORM:** Entity Framework Core 8.0
- **Database:** Azure SQL Database (SQL Server)
- **Logging:** Microsoft.Extensions.Logging
- **DI Container:** Microsoft.Extensions.DependencyInjection

## Project Structure

```
BLFramework/
├── BLFramework.csproj                 # Project file with dependencies
├── .github/
│   └── copilot-instructions.md        # Copilot setup instructions
├── README.md                           # This file
├── appsettings.json                   # Configuration and connection strings
├── Data/
│   ├── AppDbContext.cs               # EF Core DbContext
│   └── Migrations/                    # EF Core migrations folder (auto-generated)
├── Models/
│   └── BaseEntity.cs                 # Base entity with common properties
├── Services/
│   └── BaseService.cs                # Generic repository service with CRUD operations
└── Configuration/
    └── DbContextExtensions.cs        # DI extension method for DbContext
```

## Installation & Setup

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- Azure SQL Database (or SQL Server instance)

### 1. Clone or Extract Project
```bash
# Navigate to project directory
cd BLFramework
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Configure Connection String

Edit `appsettings.json` and replace the placeholder values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server.database.windows.net;Database=your-database;User Id=your-username;Password=your-password;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"
  }
}
```

**Connection String Parameters:**
- `Server` - Azure SQL Server hostname
- `Database` - Database name
- `User Id` - SQL authentication username
- `Password` - SQL authentication password
- `Encrypt=true` - Enable encryption (required for Azure)
- `TrustServerCertificate=false` - Validate server certificate

### 4. Build the Project
```bash
dotnet build
```

### 5. Create Database Migrations (if using EF migrations)
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Usage

### Setting Up Dependency Injection in Your Application

In your consuming application (Web API, Console App, etc.), add the DbContext to the DI container:

```csharp
using BLFramework.Configuration;
using BLFramework.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

// In Program.cs or Startup.cs
var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

// Add AppDbContext with your connection string
string connectionString = builder.GetConnectionString("DefaultConnection");
services.AddAppDbContext(connectionString);

// Add logging (optional)
services.AddLogging();

var serviceProvider = services.BuildServiceProvider();
```

### Creating a Custom Entity

```csharp
using BLFramework.Models;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}
```

### Configuring Entity in DbContext

```csharp
// In AppDbContext.OnModelCreating()
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        entity.Property(e => e.Price)
            .HasColumnType("decimal(18,2)");
    });
}
```

### Creating a Service for Your Entity

```csharp
using BLFramework.Services;
using BLFramework.Data;
using Microsoft.Extensions.Logging;

public class ProductService : BaseService<Product>
{
    public ProductService(AppDbContext context, ILogger<ProductService> logger)
        : base(context, logger)
    {
    }

    // Add custom business logic methods here
    public async Task<List<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Products
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync();
    }
}
```

### Register Service in DI Container

```csharp
services.AddScoped<ProductService>();
```

### Using the Service

```csharp
var serviceProvider = services.BuildServiceProvider();
var productService = serviceProvider.GetRequiredService<ProductService>();

// Get all products
var products = await productService.GetAllAsync();

// Get product by ID
var product = await productService.GetByIdAsync(1);

// Add new product
var newProduct = new Product { Name = "New Product", Price = 99.99M };
await productService.AddAsync(newProduct);

// Update product
product.Price = 79.99M;
await productService.UpdateAsync(product);

// Delete product
await productService.DeleteAsync(product);
```

## Architecture

### BaseService<T>
The `BaseService<T>` class provides:
- **GetAllAsync()** - Retrieves all entities
- **GetByIdAsync(id)** - Retrieves a single entity by ID
- **AddAsync(entity)** - Adds a new entity
- **UpdateAsync(entity)** - Updates an existing entity
- **DeleteAsync(entity)** - Deletes an entity
- **Error Handling** - Structured logging of exceptions
- **Async Operations** - All methods are async/await compatible

### DbContextExtensions
The `AddAppDbContext()` extension method configures:
- SQL Server provider for Azure SQL
- Retry policy for transient failures (up to 5 retries)
- Command timeout (30 seconds)
- Automatic SaveChangesAsync after operations

## Azure SQL Specific Features

- **Automatic Retry Policy** - Handles transient Azure SQL failures
- **Connection Pooling** - Optimized for cloud connectivity
- **Encryption Support** - TLS encryption by default
- **AAD Integration Ready** - Can be extended for Azure AD authentication

## Extending the Framework

### Creating Custom Repositories

```csharp
public class CustomRepository<T> : BaseService<T> where T : class
{
    public CustomRepository(AppDbContext context, ILogger<CustomRepository<T>> logger)
        : base(context, logger) { }

    // Add custom query methods
    public async Task<List<T>> FilterAsync(Func<T, bool> predicate)
    {
        return await Task.FromResult(
            _context.Set<T>().Where(predicate).ToList()
        );
    }
}
```

### Adding Migrations

```bash
# Create a new migration
dotnet ef migrations add YourMigrationName --startup-project YourConsoleApp

# Apply migrations to database
dotnet ef database update
```

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Connection timeout | Increase `Connection Timeout` in appsettings.json |
| Transient failures | Verify retry policy in DbContextExtensions is enabled |
| Migration errors | Ensure SQL Server provider is installed: `dotnet add package Microsoft.EntityFrameworkCore.SqlServer` |
| No tables created | Run `dotnet ef database update` after migrations |

## Contributing

To extend BLFramework:
1. Add entity models in the `Models/` folder
2. Configure entities in `AppDbContext.OnModelCreating()`
3. Create custom services inheriting from `BaseService<T>`
4. Register services in your DI container

## License

This project is part of the TaskFlow system.

## Support

For issues or questions:
- Check the troubleshooting section above
- Review Entity Framework Core documentation
- Consult Azure SQL Database documentation

---

**Last Updated:** February 5, 2026  
**Version:** 1.0.0  
**.NET Version:** 8.0
