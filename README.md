# TaskFlow Solution

TaskFlow is a comprehensive solution containing multiple projects for managing business logic and application workflows.

## Solution Structure

```
TaskFlow/
├── TaskFlow.sln                    # Solution file
├── README.md                       # This file
├── .gitignore                      # Git ignore rules
└── BLFramework/                    # Business Logic Layer
    ├── BLFramework.csproj
    ├── README.md
    ├── appsettings.json
    ├── Data/
    ├── Models/
    ├── Services/
    └── Configuration/
```

## Projects

### BLFramework
A .NET 8.0 class library providing a business logic layer with Entity Framework Core integration for Azure SQL Database connectivity.

**Features:**
- Entity Framework Core 8.0
- Azure SQL Database support with configurable connection strings
- Generic repository pattern with async CRUD operations
- Dependency injection ready
- Automatic retry policy for transient failures

See [BLFramework/README.md](BLFramework/README.md) for detailed documentation.

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code

### Build Solution
```bash
dotnet build TaskFlow.sln
```

### Restore Dependencies
```bash
dotnet restore TaskFlow.sln
```

### Build Individual Project
```bash
dotnet build BLFramework/BLFramework.csproj
```

## Configuration

Each project in the solution can have its own configuration. See individual project README files for setup instructions.

## Adding New Projects

To add a new project to the TaskFlow solution:

```bash
dotnet new classlib -n YourProjectName
dotnet sln TaskFlow.sln add YourProjectName/YourProjectName.csproj
```

## Development

- Clone or extract the repository
- Open `TaskFlow.sln` in Visual Studio 2022 or open the root folder in Visual Studio Code
- Restore dependencies: `dotnet restore`
- Build solution: `dotnet build`

## Support

Refer to individual project documentation in their respective folders.

---

**Version:** 1.0.0  
**.NET Version:** 8.0
