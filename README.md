# DaprNetFx

**Dapr SDK for .NET Framework 4.8** - Bringing cloud-native distributed application patterns to legacy .NET Framework applications.

## Overview

DaprNetFx enables .NET Framework 4.x applications to leverage [Dapr](https://dapr.io/) (Distributed Application Runtime) without requiring migration to .NET Core/.NET 8. This SDK brings modern cloud-native patterns like service invocation, state management, and pub/sub to enterprise applications that can't easily migrate.

### ⚠️ For Modern .NET Applications

**If you're using .NET Core, .NET 5+**, use the official Dapr .NET SDK instead:
- **Official SDK**: https://github.com/dapr/dotnet-sdk
- **NuGet**: `Dapr.Client`, `Dapr.AspNetCore`, `Dapr.Actors`
- **Why?**: Production-ready, actively maintained, feature-complete

**DaprNetFx is exclusively for .NET Framework 4.8 applications** that cannot migrate to modern .NET. This is a bridge solution for enterprises stuck on legacy frameworks.

### Why DaprNetFx?

- **No Migration Required**: Keep your .NET Framework 4.8 applications while gaining cloud-native capabilities
- **Production-Ready**: Built with enterprise-grade quality standards
- **Two Deployment Patterns**: Sidecar (optimal) or Remote (pragmatic) based on your hosting environment
- **Azure-Native**: Designed for Azure App Service, AKS, ACI, and hybrid deployments
- **.NET Framework 4.8 Only**: No multi-targeting, no .NET Standard - simple, focused, native

## Status

🚧 **POC Phase** - POC1 Complete, POC2 State Management Complete

### POC Roadmap

- **POC1** ✅ (Complete): Core client, service invocation (outbound), ASP.NET WebAPI DI integration, sample application
- **POC2** 🔄 (In Progress):
  - ✅ Service callbacks (inbound) - Dapr → App invocation complete
  - ✅ State Management API - Save/Get/Delete, Bulk, Transactions complete
  - ⏳ Pub/Sub, NuGet packaging, Autofac integration
- **POC3** (Planned): Production readiness, OWIN support, performance optimization, real Dapr integration tests

## Prerequisites

- .NET Framework 4.8 SDK
- Visual Studio 2019/2022 or Rider
- Dapr CLI (for local development with sidecar pattern)
- Windows OS (runtime requirement for .NET Framework)

## Quick Start

> **Note**: SDK not yet published to NuGet (planned for POC2). Build from source for now.

### Installation (POC2+)

```powershell
Install-Package DaprNetFx.Client
Install-Package DaprNetFx.AspNet  # For ASP.NET WebAPI integration
```

### Usage Examples

**Service Invocation (Console/Library)**

```csharp
using DaprNetFx;

// Default configuration (localhost:3500)
using (var daprClient = new DaprClient())
{
    var result = await daprClient.InvokeMethodAsync<MyRequest, MyResponse>(
        appId: "order-service",
        methodName: "process-order",
        request: myRequest
    );
}

// Custom configuration
var options = new DaprClientOptions
{
    HttpEndpoint = "http://localhost:3500",
    HttpTimeout = TimeSpan.FromSeconds(30),
    Required = true // Fail-fast if Dapr unavailable
};
using (var daprClient = new DaprClient(options))
{
    var result = await daprClient.InvokeMethodAsync<MyResponse>(
        appId: "inventory-service",
        methodName: "check-stock"
    );
}
```

**ASP.NET WebAPI Integration**

```csharp
// WebApiConfig.cs
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Register Dapr with dependency injection
        config.UseDapr(new DaprClientOptions
        {
            HttpEndpoint = ConfigurationManager.AppSettings["Dapr:HttpEndpoint"]
        });

        config.MapHttpAttributeRoutes();
    }
}

// Controller (Outbound)
public class OrdersController : DaprApiController
{
    public async Task<IHttpActionResult> Get(string orderId)
    {
        // Dapr property provided by base class
        var order = await Dapr.InvokeMethodAsync<Order>(
            appId: "order-service",
            methodName: $"orders/{orderId}"
        );

        return Ok(order);
    }
}
```

**Inbound Service Invocation (Dapr → App Callbacks)**

```csharp
// Receive service invocations FROM other Dapr applications
public class OrdersController : DaprApiController
{
    [DaprCallback]  // Marker attribute for callback endpoints
    [HttpPost]
    public IHttpActionResult ProcessOrder(Order order)
    {
        // Access Dapr metadata about the calling service
        var callerAppId = this.CallbackContext.CallerAppId;
        var callerNamespace = this.CallbackContext.CallerNamespace;

        // Check if request came through Dapr (vs direct HTTP)
        if (this.CallbackContext.HasDaprHeaders)
        {
            // Process order from Dapr-invoked request
            // Optional: Make outbound calls back to caller
            var result = await Dapr.InvokeMethodAsync<Inventory>(
                appId: callerAppId,
                methodName: "confirm-reservation"
            );
        }

        return Ok(new { orderId = order.Id, status = "processed" });
    }
}

// Test callback with Dapr CLI
// dapr invoke --app-id order-service --method ProcessOrder --verb POST --data '{"id":123}'
```

**State Management API**

```csharp
using DaprNetFx;

using (var client = new DaprClient())
{
    // Save state
    var order = new Order { OrderId = "order-001", Status = "pending", Total = 99.99m };
    await client.SaveStateAsync("statestore", order.OrderId, order);

    // Get state
    var retrieved = await client.GetStateAsync<Order>("statestore", order.OrderId);

    // Delete state
    await client.DeleteStateAsync("statestore", order.OrderId);
}
```

**State Management - Advanced Features**

```csharp
// Strong consistency
var inventory = await client.GetStateAsync<Inventory>(
    "statestore",
    "product-001",
    ConsistencyMode.Strong
);

// ETag concurrency control (optimistic locking)
var options = new StateOptions
{
    Concurrency = ConcurrencyMode.FirstWrite,
    ETag = "version-1",
    TtlInSeconds = 3600  // Auto-expire after 1 hour
};
await client.SaveStateAsync("statestore", "session-001", sessionData, options);

// Bulk operations
var items = new[]
{
    new StateItem<Inventory> { Key = "product-101", Value = inventory1 },
    new StateItem<Inventory> { Key = "product-102", Value = inventory2 }
};
await client.SaveBulkStateAsync("statestore", items);

var keys = new[] { "product-101", "product-102" };
var results = await client.GetBulkStateAsync<Inventory>("statestore", keys, parallelism: 5);
await client.DeleteBulkStateAsync("statestore", keys);

// State transactions (atomic multi-operation)
var operations = new[]
{
    new StateTransactionRequest
    {
        OperationType = StateOperationType.Upsert,
        Key = "account-001",
        Value = new Account { Balance = 800.00m }
    },
    new StateTransactionRequest
    {
        OperationType = StateOperationType.Upsert,
        Key = "account-002",
        Value = new Account { Balance = 700.00m }
    }
};
await client.ExecuteStateTransactionAsync("statestore", operations);
```

## Building from Source

### Clone and Build

```bash
git clone https://github.com/yourorg/DaprNetFx.git
cd DaprNetFx
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Build Requirements

- Warnings as errors enforced (`TreatWarningsAsErrors=true`)
- StyleCop analyzers enabled
- XML documentation required for public APIs
- .editorconfig rules enforced

## Developer Setup (Local Development)

### Prerequisites for SDK Development

1. **.NET Framework 4.8 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet-framework/net48
   - Verify: `dotnet --version` (shows .NET SDK version)

2. **Visual Studio 2019/2022** (recommended) or **Rider**
   - Required for .NET Framework 4.8 development
   - Ensure ".NET Framework 4.8 targeting pack" is installed

3. **Windows OS**
   - .NET Framework 4.8 is Windows-only
   - WSL2 can be used for building, but tests require Windows

### Setting Up Dapr for Local Development

The SDK's **unit tests use WireMock** (no Dapr required), but **sample applications require real Dapr**.

#### 1. Install Dapr CLI

**Windows (PowerShell)**:
```powershell
powershell -Command "iwr -useb https://raw.githubusercontent.com/dapr/cli/master/install/install.ps1 | iex"
```

**Verify Installation**:
```bash
dapr --version
# Expected: CLI version: 1.14.x, Runtime version: n/a (not initialized yet)
```

#### 2. Initialize Dapr

```bash
dapr init
```

This installs and starts:
- **Dapr runtime** (placed in `~/.dapr/bin/`)
- **Redis** (Docker container for state management and pub/sub)
- **Zipkin** (Docker container for distributed tracing)

**Verify Components Running**:
```bash
docker ps
# Should show: dapr_redis, dapr_zipkin, dapr_placement
```

**Verify Dapr Components**:
```bash
dapr components list
# Expected output:
#   NAME       TYPE             VERSION  SCOPES
#   pubsub     pubsub.redis     v1
#   statestore state.redis      v1
```

#### 3. Verify Redis (State Store)

```bash
# Connect to Redis
docker exec -it dapr_redis redis-cli

# Test Redis
127.0.0.1:6379> ping
PONG

# Exit Redis
127.0.0.1:6379> exit
```

### Running Sample Applications

**ServiceInvocationSample** (POC1):
```bash
# Terminal 1: Start Dapr sidecar
dapr run --app-id sampleapp --dapr-http-port 3500

# Terminal 2: Run sample
cd samples/ServiceInvocationSample
dotnet run
```

**StateManagementSample** (POC2):
```bash
# Terminal 1: Start Dapr sidecar
dapr run --app-id stateapp --dapr-http-port 3500

# Terminal 2: Run sample
cd samples/StateManagementSample
dotnet run
```

### Troubleshooting Local Development

**Issue**: `dapr: command not found`

**Solution**: Add Dapr to PATH
```bash
# Windows PowerShell
$env:Path += ";$env:USERPROFILE\.dapr\bin"

# Linux/macOS
export PATH=$PATH:$HOME/.dapr/bin
```

**Issue**: `Cannot connect to Docker daemon`

**Solution**: Start Docker Desktop (required for `dapr init`)

**Issue**: Redis port 6379 already in use

**Solution**: Stop conflicting Redis instance or change Dapr's Redis port
```bash
# Uninitialize Dapr
dapr uninstall

# Reinitialize with custom Redis port
dapr init --redis-port 6380
```

**Issue**: Tests failing in WSL2

**Solution**: .NET Framework 4.8 tests must run on Windows
```bash
# Build in WSL2 is OK
dotnet build

# Tests require Windows (run in PowerShell or Visual Studio)
# WSL2: dotnet test will fail (mono compatibility issues)
```

### Development Workflow

1. **Make Changes**: Edit SDK source code
2. **Build**: `dotnet build` (verify 0 errors, 0 warnings)
3. **Run Tests**: `dotnet test` (all tests must pass)
4. **Test Samples**: Run sample apps against real Dapr
5. **Commit**: Follow conventional commits (feat:, fix:, docs:, etc.)

### Code Quality Checks

Before committing:
```bash
# Build with no warnings
dotnet build --no-incremental

# Run all tests
dotnet test --no-build

# Verify StyleCop compliance (warnings as errors enforced)
# XML documentation on all public APIs
# .editorconfig rules followed
```

## Package Architecture

### Core Package (Zero DI Dependencies)

**DaprNetFx.Client** - Standalone core SDK
- Service invocation, state management, pub/sub
- No dependency injection framework required
- Manual instantiation: `new DaprClient()`
- Minimal dependencies (only Newtonsoft.Json)

### Optional Integration Packages

**DaprNetFx.AspNet** - ASP.NET WebAPI dependency injection integration
- `config.UseDapr()` fluent registration API for outbound calls
- `DaprApiController` base class with `Dapr` property (outbound)
- `DaprApiController` base class with `CallbackContext` property (inbound)
- `[DaprCallback]` attribute for marking callback endpoints
- Wraps existing IDependencyResolver (preserves user's DI setup)
- Singleton DaprClient lifecycle management

**DaprNetFx.Autofac** (POC2) - Autofac container integration
- Separate package to avoid forcing Autofac on all consumers
- Fluent registration extensions
- Lifetime management for DaprClient


**DaprNetFx.AspNet.SelfHost** (POC3) - OWIN self-hosting
- Console apps, Windows Services
- Standalone HTTP listener

**Why Separate Packages?**
- Core SDK remains lightweight
- Consumers choose only what they need
- No forced dependency on specific DI frameworks
- Follows official Dapr SDK pattern

## Project Structure

```
DaprNetFx/
├── src/
│   ├── DaprNetFx.Client/              # Core SDK (zero DI deps) ✅
│   │   ├── Service Invocation API     # InvokeMethodAsync<TRequest, TResponse>
│   │   ├── State Management API       # SaveStateAsync, GetStateAsync, DeleteStateAsync
│   │   ├── Bulk State Operations      # SaveBulkStateAsync, GetBulkStateAsync, DeleteBulkStateAsync
│   │   └── State Transactions         # ExecuteStateTransactionAsync (atomic multi-op)
│   ├── DaprNetFx.AspNet/              # ASP.NET WebAPI DI integration ✅
│   │   ├── Dependency Injection       # config.UseDapr()
│   │   ├── Outbound Calls             # DaprApiController with Dapr property
│   │   └── Inbound Callbacks          # CallbackContext, [DaprCallback] attribute
│   ├── DaprNetFx.Autofac/             # Autofac integration (POC2 - planned)
│   └── DaprNetFx.AspNet.SelfHost/     # OWIN self-host adapter (POC3 - planned)
├── test/
│   ├── DaprNetFx.Client.Tests/        # Client unit tests (WireMock) ✅
│   │   ├── Service Invocation Tests   # 21 tests - all HTTP methods, error handling
│   │   └── State Management Tests     # 50 tests - CRUD, bulk, transactions, validation
│   ├── DaprNetFx.AspNet.Tests/        # AspNet unit tests (FakeItEasy) ✅
│   │   ├── DI Integration Tests       # 11 tests - resolver, scope, disposal
│   │   └── Callback Tests             # 25 tests - context, headers, routing
│   └── DaprNetFx.IntegrationTests/    # Real Dapr integration tests (POC3 - planned)
├── samples/
│   ├── ServiceInvocationSample/       # Console app - service invocation patterns ✅
│   ├── StateManagementSample/         # Console app - state CRUD, bulk, transactions ✅
│   └── PubSubSample/                  # POC2 sample (planned)
└── docs/
    └── ARCHITECTURE.md                # Deployment patterns, Azure hosting options
```

**Current Status**:
- ✅ **107 tests passing** (71 client + 36 AspNet)
- ✅ **2 complete sample applications** with comprehensive READMEs
- ✅ **Service Invocation** - Outbound calls + Inbound callbacks
- ✅ **State Management** - Full API including bulk operations and transactions
- ⏳ **Pub/Sub** - Planned for POC2
- ⏳ **NuGet Packaging** - Planned for POC2

## Deployment Patterns

### Sidecar Pattern (Optimal)

Dapr runs co-located with your application (localhost, ~1ms latency).

**Supported On**:
- Azure Kubernetes Service (AKS) with Windows nodes
- Azure Container Instances (ACI)
- Self-hosted (IIS, Windows Server)

### Remote Pattern (Pragmatic)

Dapr runs on separate infrastructure, accessed over network (5-50ms latency).

**Supported On**:
- Azure App Service (Windows)
- Traditional IIS hosting
- Hybrid cloud scenarios

## Configuration

Configuration follows standard .NET Framework patterns:

```xml
<appSettings>
  <!-- Dapr endpoint (defaults to localhost:3500 for sidecar) -->
  <add key="Dapr:HttpEndpoint" value="http://localhost:3500" />

  <!-- Fail-fast if Dapr unavailable (default: true) -->
  <add key="Dapr:Required" value="true" />

  <!-- API token authentication (optional) -->
  <add key="Dapr:ApiToken" value="your-token-here" />
</appSettings>
```

Environment variables override app.config values:
- `DAPR_HTTP_ENDPOINT`
- `DAPR_REQUIRED`
- `DAPR_API_TOKEN`

## Contributing

This project is in POC phase. Contribution guidelines will be published when we reach production readiness (post-POC3).

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by the official [Dapr .NET SDK](https://github.com/dapr/dotnet-sdk)
- Built for the enterprise .NET Framework community
- Follows .NET Framework best practices and constraints

## Resources

- [Dapr Documentation](https://docs.dapr.io/)
- [.NET Framework 4.8 Documentation](https://docs.microsoft.com/en-us/dotnet/framework/)
- [Azure Deployment Patterns](docs/deployment-patterns.md) *(future)*

---

**Status**: 🚧 POC2 in progress - State Management complete (107 tests passing) - Not ready for production use
