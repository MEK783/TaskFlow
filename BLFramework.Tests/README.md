# BLFramework.Tests

Unit testing project for the BLFramework library.

## Framework & Tools

- **Testing Framework**: xUnit
- **Mocking Library**: Moq
- **.NET Version**: net8.0

## Running Tests

From the project root directory:

```bash
dotnet test BLFramework.Tests
```

Or run tests with verbose output:

```bash
dotnet test BLFramework.Tests --verbosity detailed
```

## Test Structure

Tests are organized by the components they test:

- `BaseEntityTests.cs` - Tests for the `BaseEntity` model
- `PasswordHashingServiceTests.cs` - Tests for the `PasswordHashingService`

## Writing Tests

### Basic Test Structure

```csharp
[Fact]
public void MethodName_ShouldDoSomething()
{
    // Arrange
    var input = "test";
    
    // Act
    var result = MyClass.MyMethod(input);
    
    // Assert
    Assert.Equal(expected, result);
}
```

### Using Mocks

```csharp
private readonly PasswordHashingService _service;
private readonly Mock<ILogger<PasswordHashingService>> _mockLogger;

public PasswordHashingServiceTests()
{
    _mockLogger = new Mock<ILogger<PasswordHashingService>>();
    _service = new PasswordHashingService(_mockLogger.Object);
}
```

## Common Assertions

- `Assert.True(condition)` - Assert condition is true
- `Assert.False(condition)` - Assert condition is false
- `Assert.Equal(expected, actual)` - Assert equality
- `Assert.NotEqual(unexpected, actual)` - Assert inequality
- `Assert.Null(obj)` - Assert object is null
- `Assert.NotNull(obj)` - Assert object is not null
- `Assert.Throws<ExceptionType>(() => { /* code */ })` - Assert exception is thrown

## Resources

- [xUnit Documentation](https://xunit.net/docs/getting-started/netfx)
- [Moq Documentation](https://github.com/moq/moq4/wiki/Quickstart)
