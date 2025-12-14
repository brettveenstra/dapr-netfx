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

🚧 **POC Phase** - Currently implementing POC1 (Bidirectional Basics)

### POC Roadmap

- **POC1** (In Progress): Service invocation bidirectional communication
- **POC2** (Planned): State Management, Pub/Sub, NuGet packaging
- **POC3** (Planned): Production readiness, OWIN support, performance optimization

## Prerequisites

- .NET Framework 4.8 SDK
- Visual Studio 2019/2022 or Rider
- Dapr CLI (for local development with sidecar pattern)
- Windows OS (runtime requirement for .NET Framework)

## Quick Start

> **Note**: SDK not yet published. This section will be updated when NuGet packages are available (POC2).

### Installation (Future)

```powershell
Install-Package DaprNetFx.Client
Install-Package DaprNetFx.AspNet  # For ASP.NET WebAPI integration
```

### Usage Example (Future)

```csharp
// Service invocation example
var daprClient = new DaprClient();
var result = await daprClient.InvokeMethodAsync<MyRequest, MyResponse>(
    "target-service",
    "method-name",
    request
);
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

## Package Architecture

### Core Package (Zero DI Dependencies)

**DaprNetFx.Client** - Standalone core SDK
- Service invocation, state management, pub/sub
- No dependency injection framework required
- Manual instantiation: `new DaprClient()`
- Minimal dependencies (only Newtonsoft.Json)

### Optional Integration Packages

**DaprNetFx.AspNet** - ASP.NET WebAPI integration
- Dapr → App callbacks via WebAPI
- Message handlers for Dapr headers
- Attribute routing support

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
│   ├── DaprNetFx.Client/              # Core SDK (zero DI deps)
│   ├── DaprNetFx.AspNet/              # ASP.NET WebAPI adapter
│   ├── DaprNetFx.Autofac/             # Autofac integration (POC2)
│   └── DaprNetFx.AspNet.SelfHost/     # OWIN self-host adapter (POC3)
├── test/
│   ├── DaprNetFx.Client.Tests/        # Unit tests (WireMock)
│   └── DaprNetFx.Client.IntegrationTests/  # Integration tests (POC3)
├── samples/
│   ├── ServiceInvocationSample/       # POC1 sample
│   ├── StateManagementSample/         # POC2 sample
│   └── PubSubSample/                  # POC2 sample
└── docs/
    └── ARCHITECTURE.md                # Architecture documentation
```

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

**Status**: 🚧 POC1 in progress - Not ready for production use
