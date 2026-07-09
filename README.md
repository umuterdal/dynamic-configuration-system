# Dynamic Configuration System

A production-ready .NET 8 configuration management system with MongoDB storage, in-memory caching, and automatic background refresh.

## Features

- **Dynamic Configuration**: Runtime configuration updates without deployment
- **Service Isolation**: Each application only accesses its own configurations
- **Type Safety**: Generic `GetValue<T>()` with automatic type conversion
- **Thread Safety**: ConcurrentDictionary for safe concurrent access
- **Automatic Refresh**: BackgroundService with PeriodicTimer
- **Graceful Degradation**: Continues working when storage is unavailable
- **Admin Panel**: Web UI for CRUD operations
- **Docker Support**: Complete docker-compose setup

## Project Structure

```
DynamicConfiguration.sln
├── src/
│   ├── Configuration.Domain/          # Entities, Interfaces, DTOs
│   ├── Configuration.Application/     # Business Logic, Services
│   ├── Configuration.Infrastructure/  # MongoDB Repository
│   ├── Configuration.Library/         # ConfigurationReader Public API
│   ├── Configuration.Admin/           # ASP.NET Core MVC Admin Panel
│   ├── Configuration.DemoApi/         # Demo API Application
│   └── Configuration.Shared/          # Common Utilities
├── tests/
│   └── Configuration.UnitTests/       # xUnit + Moq Tests
└── docker-compose.yml
```

## Quick Start

### Prerequisites

- .NET 8 SDK
- Docker & Docker Compose

### Running with Docker

```bash
# Start all services
docker-compose up -d

# Access applications
# Admin Panel: http://localhost:5000
# Demo API: http://localhost:5001/swagger
```

### Running Locally

```bash
# Start MongoDB
docker run -d -p 27017:27017 --name mongodb mongo:7.0

# Run Admin Panel
dotnet run --project src/Configuration.Admin

# Run Demo API
dotnet run --project src/Configuration.DemoApi
```

## ConfigurationReader Usage

```csharp
// Initialize
var reader = new ConfigurationReader(
    applicationName: "MY-SERVICE",
    repository: repository,
    refreshTimerIntervalInMs: 30000,
    logger: logger);

// Get values
string siteName = reader.GetValue<string>("SiteName");
int maxItems = reader.GetValue<int>("MaxItemCount");
bool isEnabled = reader.GetValue<bool>("IsBasketEnabled");
double taxRate = reader.GetValue<double>("TaxRate");

// Try get (safe access)
if (reader.TryGetValue<string>("SiteName", out var name))
{
    Console.WriteLine(name);
}

// Get all values
var allValues = reader.GetAllValues();

// Force refresh
await reader.RefreshAsync();
```

## API Endpoints (Demo API)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/demo/value/{key}` | Get string value by key |
| GET | `/api/demo/typed/{key}?type=int` | Get typed value |
| GET | `/api/demo/all` | Get all configurations |
| POST | `/api/demo/refresh` | Force cache refresh |

## Configuration

### appsettings.json

```json
{
  "ConfigurationReader": {
    "ApplicationName": "SERVICE-A",
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "ConfigurationDb",
    "CollectionName": "Configurations",
    "RefreshTimerIntervalInMs": 30000
  }
}
```

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Architecture

### Clean Architecture

- **Domain**: Entities, interfaces, DTOs - no external dependencies
- **Application**: Business logic, services
- **Infrastructure**: MongoDB repository implementation
- **Library**: Public API (ConfigurationReader)

### Key Design Decisions

1. **ConcurrentDictionary**: O(1) lookups, thread-safe, no external dependencies
2. **BackgroundService + PeriodicTimer**: Built-in lifecycle management, graceful shutdown
3. **MongoDB**: Schema flexibility, good .NET support, scalable
4. **Repository Pattern**: Testability, separation of concerns

## Documentation

- [Architecture.md](Architecture.md) - Detailed architecture decisions
- [ADR.md](ADR.md) - Architecture Decision Records

## License

MIT
