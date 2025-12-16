# State Management Sample

Demonstrates Dapr State Management API usage with the DaprNetFx SDK (.NET Framework 4.8).

## What This Sample Shows

This console application demonstrates **6 comprehensive examples**:

1. **Basic CRUD Operations** - Save, Get, Update, Delete state
2. **Strong Consistency** - Read-your-writes guarantees
3. **ETag Concurrency Control** - Optimistic locking with first-write wins
4. **Bulk Operations** - Save/Get/Delete multiple items in parallel
5. **State Transactions** - Atomic multi-operation (e.g., fund transfers)
6. **TTL Support** - Time-to-live for auto-expiring state

## Prerequisites

### 1. Dapr CLI Installed

**Windows (PowerShell)**:
```powershell
powershell -Command "iwr -useb https://raw.githubusercontent.com/dapr/cli/master/install/install.ps1 | iex"
```

**Linux/macOS**:
```bash
wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash
```

**Verify Installation**:
```bash
dapr --version
# Expected: CLI version: 1.14.x Runtime version: 1.14.x
```

### 2. Dapr Initialized

```bash
dapr init
```

This installs:
- Dapr runtime binaries
- Redis (for state management)
- Zipkin (for tracing)

**Verify Components**:
```bash
dapr components list
# Expected: statestore (state.redis), pubsub (pubsub.redis)
```

### 3. .NET Framework 4.8 SDK

Download from: https://dotnet.microsoft.com/download/dotnet-framework/net48

## Running the Sample

### Step 1: Start Dapr Sidecar

Open a terminal and run:

```bash
dapr run --app-id stateapp --dapr-http-port 3500
```

This starts:
- **Dapr sidecar** listening on `http://localhost:3500`
- **Default state store** (Redis) configured as `statestore`

**Keep this terminal open** while running the sample.

### Step 2: Build and Run the Sample

In a **second terminal**:

```bash
cd samples/StateManagementSample
dotnet build
dotnet run
```

Or from Visual Studio:
1. Open `DaprNetFx.sln`
2. Set `StateManagementSample` as startup project
3. Press F5

### Expected Output

```
=== DaprNetFx State Management Sample ===

Example 1: Basic CRUD Operations
---------------------------------
Saving order: order-001
✓ Order saved
Reading order: order-001
✓ Order retrieved: Status=pending, Total=$99.99
Updating order status to: confirmed
✓ Order updated
Deleting order: order-001
✓ Order deleted
✓ Verified order was deleted (null returned)

Example 2: Strong Consistency
-----------------------------
Saving inventory with eventual consistency (default)
✓ Saved with eventual consistency
Reading inventory with strong consistency
✓ Retrieved with strong consistency: Quantity=100

Example 3: ETag Concurrency Control (Optimistic Locking)
--------------------------------------------------------
Creating account: account-001 with balance $1000
✓ Account created

Simulating concurrent updates with first-write wins:
  Update 1: Attempting to set balance to $1100
  ✓ Update 1 succeeded

Example 4: Bulk Operations
--------------------------
Bulk saving 3 inventory items
✓ Saved 3 items in bulk

Bulk reading 3 inventory items (parallelism=5)
✓ Retrieved 3 items in bulk
  - product-101: Quantity=50
  - product-102: Quantity=75
  - product-103: Quantity=100

Bulk deleting 3 inventory items
✓ Deleted 3 items in bulk

Example 5: State Transactions (Atomic Multi-Operation)
------------------------------------------------------
Setting up accounts:
  account-tx-001: $1000
  account-tx-002: $500
✓ Accounts created

Transferring $200 from account-tx-001 to account-tx-002
✓ Transaction completed atomically

Final balances:
  account-tx-001: $800
  account-tx-002: $700

Example 6: TTL Support (Time-To-Live)
-------------------------------------
Saving session with TTL: 60 seconds
✓ Session saved with 60s TTL
  (Session will automatically expire after 60 seconds)
✓ Session retrieved immediately: UserId=user-123

✓ All examples completed successfully!

Press any key to exit...
```

## Code Walkthrough

### Example 1: Basic CRUD

```csharp
// Save state
var order = new OrderState { OrderId = "order-001", Status = "pending", Total = 99.99m };
await client.SaveStateAsync("statestore", order.OrderId, order);

// Get state
var retrieved = await client.GetStateAsync<OrderState>("statestore", order.OrderId);

// Update state (just save again with new values)
retrieved.Status = "confirmed";
await client.SaveStateAsync("statestore", retrieved.OrderId, retrieved);

// Delete state
await client.DeleteStateAsync("statestore", order.OrderId);
```

### Example 3: ETag Concurrency

```csharp
var options = new StateOptions
{
    Concurrency = ConcurrencyMode.FirstWrite,
    ETag = "version-1" // From previous read
};

await client.SaveStateAsync("statestore", "account-001", account, options);
// Fails with DaprException if ETag doesn't match current version
```

### Example 4: Bulk Operations

```csharp
// Save multiple items at once
var items = new[]
{
    new StateItem<InventoryState> { Key = "product-101", Value = inventory1 },
    new StateItem<InventoryState> { Key = "product-102", Value = inventory2 }
};
await client.SaveBulkStateAsync("statestore", items);

// Get multiple items with parallelism
var keys = new[] { "product-101", "product-102" };
var results = await client.GetBulkStateAsync<InventoryState>("statestore", keys, parallelism: 5);

// Delete multiple items
await client.DeleteBulkStateAsync("statestore", keys);
```

### Example 5: State Transactions

```csharp
// All operations succeed or fail together (atomic)
var operations = new[]
{
    new StateTransactionRequest
    {
        OperationType = StateOperationType.Upsert,
        Key = "account-001",
        Value = new AccountState { Balance = 800.00m }
    },
    new StateTransactionRequest
    {
        OperationType = StateOperationType.Upsert,
        Key = "account-002",
        Value = new AccountState { Balance = 700.00m }
    }
};

await client.ExecuteStateTransactionAsync("statestore", operations);
```

### Example 6: TTL Support

```csharp
var options = new StateOptions
{
    TtlInSeconds = 60 // Automatically delete after 60 seconds
};

await client.SaveStateAsync("statestore", "session-001", session, options);
// State will expire automatically - no manual cleanup needed
```

## Troubleshooting

### Error: "Failed to communicate with Dapr"

**Cause**: Dapr sidecar is not running.

**Solution**: Start Dapr sidecar:
```bash
dapr run --app-id stateapp --dapr-http-port 3500
```

### Error: "state store statestore not found"

**Cause**: Dapr components not initialized.

**Solution**:
```bash
dapr init
```

Verify Redis is running:
```bash
docker ps
# Should show dapr_redis container
```

### State Not Persisting Between Runs

**Expected Behavior**: Redis stores state in-memory by default (development mode).

**For Production**: Configure Redis with persistence in `components/statestore.yaml`:
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: enableTLS
    value: false
  - name: redisPassword
    secretKeyRef:
      name: redis-secret
      key: password
```

### Transactions Failing

**Cause**: Not all state stores support transactions.

**Supported Stores**:
- Redis ✓
- MongoDB ✓
- PostgreSQL ✓
- Azure Cosmos DB ✓

**Solution**: Ensure you're using a transactional state store. Check Dapr docs:
https://docs.dapr.io/reference/components-reference/supported-state-stores/

## State Store Configuration

The sample uses the **default state store** (`statestore`) configured by `dapr init`.

### Viewing Your State Store Config

```bash
cat ~/.dapr/components/statestore.yaml
```

### Using a Custom State Store

Create `components/my-statestore.yaml`:
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: mystore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
```

Update `StoreName` constant in `Program.cs`:
```csharp
private const string StoreName = "mystore";
```

Run with custom components:
```bash
dapr run --app-id stateapp --dapr-http-port 3500 --resources-path ./components
```

## Learn More

- [Dapr State Management Docs](https://docs.dapr.io/developing-applications/building-blocks/state-management/)
- [State Store Components](https://docs.dapr.io/reference/components-reference/supported-state-stores/)
- [DaprNetFx API Reference](../../README.md)
- [State Management Best Practices](https://docs.dapr.io/developing-applications/building-blocks/state-management/howto-get-save-state/)

## Next Steps

1. Try modifying the TTL values in Example 6
2. Experiment with different consistency modes
3. Implement a multi-step workflow using transactions
4. Configure a production state store (Azure Cosmos DB, PostgreSQL)
5. Add your own data models and business logic
