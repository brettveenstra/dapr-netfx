# Service Invocation Sample

Demonstrates Dapr service-to-service invocation from .NET Framework 4.8 console application.

## What This Sample Shows

- **Default Configuration**: Using `DaprClient` with default settings (localhost:3500)
- **Custom Configuration**: Configuring HTTP endpoint, timeout, and API token
- **Optional Dapr Mode**: Running application with degraded functionality when Dapr is unavailable
- **Request/Response Patterns**: Invoking methods with typed request/response models
- **Error Handling**: Catching `DaprException` when sidecar is unavailable

## Prerequisites

1. **Install Dapr CLI**
   ```bash
   # Windows (PowerShell)
   powershell -Command "iwr -useb https://raw.githubusercontent.com/dapr/cli/master/install/install.ps1 | iex"

   # Linux/macOS
   wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash
   ```

2. **Initialize Dapr**
   ```bash
   dapr init
   ```

3. **Verify Installation**
   ```bash
   dapr --version
   ```

## Running the Sample

### Option 1: Quick Test (Sample Only)

Run the sample with Dapr sidecar attached:

```bash
cd samples/ServiceInvocationSample/bin/Debug/net48

dapr run --app-id service-invocation-sample --dapr-http-port 3500 -- ServiceInvocationSample.exe
```

**Expected Output**: Service invocation calls will fail gracefully (no target service running), demonstrating error handling.

### Option 2: Full Demo (With Target Service)

For a complete demonstration, you'll need a target service. Create a simple HTTP service:

**Step 1**: Create a mock service (Python example):

```python
# order_processor.py
from flask import Flask, request, jsonify

app = Flask(__name__)

@app.route('/process', methods=['POST'])
def process_order():
    order = request.json
    return jsonify({
        'orderId': order['orderId'],
        'status': 'Processed',
        'totalPrice': order['quantity'] * 29.99
    })

@app.route('/health', methods=['GET'])
def health():
    return jsonify({'status': 'Healthy'})

@app.route('/status', methods=['GET'])
def status():
    return 'Running', 200

if __name__ == '__main__':
    app.run(port=5001)
```

**Step 2**: Start the target service with Dapr:

```bash
# Terminal 1 - Start order-processor service
dapr run --app-id order-processor --app-port 5001 --dapr-http-port 3501 -- python order_processor.py
```

**Step 3**: Run the sample:

```bash
# Terminal 2 - Run sample application
cd samples/ServiceInvocationSample/bin/Debug/net48
dapr run --app-id service-invocation-sample --dapr-http-port 3500 -- ServiceInvocationSample.exe
```

**Expected Output**: Successful service invocations with JSON responses.

### Option 3: Without Dapr (Demonstrates Required=false)

Run the sample executable directly without Dapr sidecar:

```bash
cd samples/ServiceInvocationSample/bin/Debug/net48
ServiceInvocationSample.exe
```

**Expected Output**:
- Examples 1 & 2 fail with `DaprException` (Required=true)
- Example 3 handles gracefully (Required=false)

## Configuration Options

```csharp
var options = new DaprClientOptions
{
    // Dapr sidecar HTTP endpoint
    HttpEndpoint = "http://localhost:3500",

    // API token for Dapr API authentication (if enabled)
    ApiToken = null,

    // Timeout for HTTP requests
    HttpTimeout = TimeSpan.FromSeconds(30),

    // Throw exception if Dapr is unavailable
    Required = true
};
```

## Key Concepts

### Service Invocation

Dapr enables service-to-service calls with built-in:
- Service discovery (no hardcoded URLs)
- Automatic retries
- Distributed tracing
- mTLS encryption

```csharp
// Instead of: HttpClient.PostAsync("http://order-service:5001/process", ...)
// Use Dapr:
var response = await daprClient.InvokeMethodAsync<OrderRequest, OrderResponse>(
    appId: "order-processor",
    methodName: "process",
    data: request);
```

### Deployment Patterns

This sample demonstrates **sidecar pattern** (Dapr running on localhost):

```
┌─────────────────────────────┐
│   ServiceInvocationSample   │
│    (This Application)       │
│         :3500               │
└──────────┬──────────────────┘
           │ HTTP
           ▼
┌─────────────────────────────┐
│   Dapr Sidecar (localhost)  │
│         :3500               │
└──────────┬──────────────────┘
           │ Service Discovery
           ▼
┌─────────────────────────────┐
│  order-processor's Sidecar  │
│         :3501               │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│   order-processor Service   │
│         :5001               │
└─────────────────────────────┘
```

For production deployments on Azure, see `/docs/ARCHITECTURE.md` for remote Dapr patterns (App Service, AKS).

## Troubleshooting

### "Failed to communicate with Dapr"

**Cause**: Dapr sidecar not running or incorrect port

**Solution**:
```bash
# Check Dapr is running
dapr list

# Verify HTTP port matches DaprClientOptions.HttpEndpoint
# Default: http://localhost:3500
```

### "Service not available"

**Cause**: Target service (`order-processor`) not registered with Dapr

**Solution**:
```bash
# Verify target service is running
dapr list

# Should show both apps:
# - service-invocation-sample (port 3500)
# - order-processor (port 3501)
```

### Build Errors

**Cause**: Project reference or NuGet package issues

**Solution**:
```bash
cd /path/to/DaprNetFx
dotnet restore
dotnet build samples/ServiceInvocationSample/ServiceInvocationSample.csproj
```

## Next Steps

- **State Management**: See `/samples/StateManagementSample` (future)
- **Pub/Sub**: See `/samples/PubSubSample` (future)
- **ASP.NET Web API Integration**: See `src/DaprNetFx.AspNet` for Web API controllers
- **Production Deployment**: See `/docs/ARCHITECTURE.md` for Azure deployment patterns

## Resources

- [Dapr Documentation](https://docs.dapr.io/)
- [Service Invocation Building Block](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/)
- [Dapr .NET SDK](https://github.com/dapr/dotnet-sdk) (for .NET Core comparison)
