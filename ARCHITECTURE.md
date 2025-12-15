# DaprNetFx Architecture

## Strategic Positioning

### Why DaprNetFx Exists

**Official Dapr .NET SDK** (https://github.com/dapr/dotnet-sdk):
- Targets: .NET Core 2.0+, .NET 5+
- Status: Production-ready, actively maintained by Dapr community
- Use case: **All modern .NET applications should use this**

**DaprNetFx SDK** (this repository):
- Targets: **.NET Framework 4.8 ONLY**
- Status: Bridge solution for legacy applications
- Use case: **Enterprises stuck on .NET Framework that cannot migrate**

### Framework Targeting

**EXCLUSIVELY .NET Framework 4.8**

All projects in this solution use:
```xml
<TargetFramework>net48</TargetFramework>
```

**No .NET Standard, no multi-targeting, no modern .NET support.**

**Rationale**:
1. Anyone who can use modern .NET should use official Dapr SDK
2. Native .NET Framework APIs (ConfigurationManager, System.Web.Http, ServicePointManager)
3. Simplified development and testing (single target framework)
4. Clear scope: bridge solution, not long-term platform

## SDK Architecture

### Package Structure

**Core Package** (zero dependencies on DI frameworks):
```
DaprNetFx.Client              → Core Dapr client (service invocation, state, pub/sub)
                                 - Standalone, no DI framework required
                                 - Manual instantiation: new DaprClient()
```

**Integration Packages** (optional, consumer choice):
```
DaprNetFx.AspNet              → ASP.NET WebAPI integration
DaprNetFx.AspNet.SelfHost     → OWIN self-hosting support (POC3)
DaprNetFx.Autofac             → Autofac container integration (POC2/3)
```

**Rationale**:
- Core SDK has **no DI framework dependencies** (Autofac, Unity, etc.)
- Consumers choose integration packages based on their stack
- Prevents forcing Autofac DLLs on consumers who don't use it
- Follows official Dapr SDK pattern (Dapr.Client vs Dapr.AspNetCore)

### Technology Stack

**Runtime**: .NET Framework 4.8 (Windows-only)

**HTTP Client**:
- Singleton `HttpClient` pattern (no `HttpClientFactory` in .NET Framework)
- `ServicePointManager` configuration for connection pooling
- Manual HTTP lifetime management

**Serialization**:
- Newtonsoft.Json (de facto standard for .NET Framework)
- Not System.Text.Json (not available in .NET Framework)

**Configuration**:
- `ConfigurationManager.AppSettings` (app.config/web.config)
- Environment variable overrides
- Not Microsoft.Extensions.Configuration

**Dependency Injection** (optional integration packages):
- Core SDK: No DI framework required (manual instantiation)
- `DaprNetFx.Autofac`: Autofac 9.x integration (separate package)
- Consumers can integrate with any DI framework (Unity, Castle Windsor, etc.)

**Testing**:
- NUnit 4.x
- Shouldly for assertions
- FakeItEasy for mocking
- WireMock.Net for HTTP mocking

## Deployment Patterns

### Sidecar Pattern (Optimal)

Dapr runs co-located with application (localhost, ~1ms latency).

**Hosting Options**:
- Azure Kubernetes Service (AKS) with **Windows nodes**
- Azure Container Instances (ACI)
- Self-hosted (IIS, Windows Server)

**Configuration**:
```xml
<add key="Dapr:HttpEndpoint" value="http://localhost:3500" />
```

### Remote Pattern (Pragmatic)

Dapr runs on separate infrastructure, accessed over network (5-50ms latency).

**Hosting Options**:
- Azure App Service (Windows) → Remote Dapr on AKS
- Traditional IIS hosting → Remote Dapr cluster
- Hybrid cloud scenarios

**Configuration**:
```xml
<add key="Dapr:HttpEndpoint" value="https://dapr-cluster.example.com" />
<add key="Dapr:ApiToken" value="your-token" />
```

## Azure Compatibility

### Supported Azure Services

| Service | Pattern | Notes |
|---------|---------|-------|
| **AKS (Windows nodes)** | Sidecar | ✅ Optimal - co-located Dapr |
| **Azure Container Instances** | Sidecar | ✅ POC/dev workloads |
| **App Service (Windows)** | Remote | ⚠️ Sidecars not supported on Windows App Service |
| **Azure VMs** | Sidecar | ✅ Full control |

### NOT Supported

| Service | Reason |
|---------|--------|
| **Azure Container Apps** | Linux-only (no .NET Framework support) |
| **App Service Linux** | .NET Framework is Windows-only |

## Design Principles

### 1. Native .NET Framework Patterns

Use Framework-native APIs, not .NET Core backports:
- ✅ `ConfigurationManager.AppSettings`
- ✅ `System.Web.Http.ApiController`
- ✅ `ServicePointManager.DefaultConnectionLimit`
- ❌ `IConfiguration`, `IOptions<T>`
- ❌ `IHttpClientFactory`

### 2. Fail-Fast by Default

```xml
<add key="Dapr:Required" value="true" />  <!-- Default -->
```

When Dapr unavailable:
- Throw `DaprException` with clear setup instructions
- Don't silently degrade (prevents production issues)
- Opt-in graceful degradation: `Dapr:Required=false`

### 3. Configuration Precedence

```
Environment Variables > app.config > Defaults
```

Example:
1. Check `DAPR_HTTP_ENDPOINT` env var
2. Check `ConfigurationManager.AppSettings["Dapr:HttpEndpoint"]`
3. Default to `http://localhost:3500`

### 4. Production-Grade Quality

- `TreatWarningsAsErrors=true` (non-negotiable)
- StyleCop + NetAnalyzers enforced
- XML documentation on all public APIs
- Comprehensive test coverage (unit + integration)
- Build automation, CI/CD ready

## Implementation Constraints

### .NET Framework 4.8 Limitations

**Cannot Use** (these are .NET Core/.NET 5+ features):
- `Span<T>`, `Memory<T>` (zero-allocation patterns)
- `IAsyncEnumerable<T>` (streaming)
- `System.Text.Json` (use Newtonsoft.Json)
- `HttpClientFactory` (use singleton HttpClient)
- `ValueTask<T>` (limited support)

**Must Use** (Framework alternatives):
```csharp
// Singleton HttpClient
private static readonly HttpClient _httpClient = new HttpClient();

// ServicePointManager for connection pooling
ServicePointManager.DefaultConnectionLimit = 50;

// Newtonsoft.Json for serialization
JsonConvert.SerializeObject(obj);
```

### ASP.NET WebAPI Integration

POC1 targets ASP.NET WebAPI as universal entry point:
- Dapr → App callbacks via WebAPI endpoints
- Attribute routing: `[Route("api/invoke")]`
- Message handlers for Dapr headers
- Compatible with Web Forms, MVC, OWIN hosting

## Security Considerations

### Authentication

**Localhost (Sidecar)**:
- No authentication required (localhost trust boundary)
- Dapr and app on same machine

**Remote Dapr**:
- API Token authentication required
- `Dapr-Api-Token` header
- Configuration: `Dapr:ApiToken` setting

### Future (POC3 stretch)**:
- Azure Managed Identity integration
- Certificate-based authentication

## Performance Targets

### POC3 Goals

- **Sidecar latency**: < 5ms overhead (app → Dapr → app)
- **Remote latency**: < 50ms overhead (network-dependent)
- **Throughput**: > 1000 req/sec (sidecar pattern)
- **Memory**: < 50MB SDK overhead

### Optimization Strategies

1. **HttpClient Reuse**: Singleton pattern, connection pooling
2. **ServicePointManager**: Tune `DefaultConnectionLimit`
3. **Serialization**: Consider protobuf for high-throughput scenarios
4. **Async/Await**: Minimize thread pool contention
5. **Configuration**: Cache AppSettings reads

## POC Roadmap

### POC1: Bidirectional Basics (In Progress)

**Goal**: Prove .NET Framework ↔ Dapr works

**Scope**:
- Service invocation (App → Dapr, Dapr → App)
- WebAPI adapter
- WireMock testing
- Visual Studio F5 debug

**Deliverables**:
- `DaprNetFx.Client`
- `DaprNetFx.AspNet`
- `ServiceInvocationSample`
- Unit tests (WireMock)

### POC2: Building Blocks + Packaging

**Goal**: Add state, pub/sub, NuGet packaging, DI integration

**Scope**:
- State Management API
- Pub/Sub API
- NuGet packages validated
- `DaprNetFx.Autofac` integration package (optional)
- Focused samples

### POC3: Production Readiness

**Goal**: OWIN, performance, real Dapr, Azure

**Scope**:
- OWIN self-host adapter
- Performance < 100ms overhead
- Real Dapr integration tests
- Azure deployment (ACI or AKS)

## Migration Path (Future)

When enterprises finally migrate to modern .NET:

1. **Replace DaprNetFx with official SDK**:
   ```diff
   - Install-Package DaprNetFx.Client
   + Install-Package Dapr.Client
   ```

2. **Update namespaces**:
   ```diff
   - using DaprNetFx;
   + using Dapr.Client;
   ```

3. **Refactor configuration** (AppSettings → IConfiguration)

4. **Refactor WebAPI** (ASP.NET → ASP.NET Core)

**Design goal**: API surface as similar to official SDK as possible to ease migration.

## References

- [Official Dapr .NET SDK](https://github.com/dapr/dotnet-sdk)
- [Dapr Documentation](https://docs.dapr.io/)
- [.NET Framework 4.8](https://docs.microsoft.com/en-us/dotnet/framework/)
- [Azure AKS Windows Containers](https://docs.microsoft.com/en-us/azure/aks/windows-container-overview)

---

**Key Principle**: This SDK exists ONLY for .NET Framework 4.8 applications that cannot migrate to modern .NET. For all other scenarios, use the official Dapr .NET SDK.
