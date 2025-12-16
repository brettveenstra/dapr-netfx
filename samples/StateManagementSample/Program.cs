// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Samples.StateManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Sample application demonstrating Dapr State Management API usage.
    /// </summary>
    public class Program
    {
        private const string StoreName = "statestore";

        public static void Main(string[] args)
        {
            Console.WriteLine("=== DaprNetFx State Management Sample ===\n");

            try
            {
                RunExamplesAsync().GetAwaiter().GetResult();

                WriteSuccess("\n✓ All examples completed successfully!");
            }
            catch (DaprException ex)
            {
                WriteError($"\n✗ Dapr Error: {ex.Message}");
                WriteWarning("\nEnsure Dapr sidecar is running:");
                WriteWarning("  dapr run --app-id stateapp --dapr-http-port 3500");
                Environment.ExitCode = 1;
            }
            catch (Exception ex)
            {
                WriteError($"\n✗ Unexpected Error: {ex.Message}");
                Environment.ExitCode = 1;
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static async Task RunExamplesAsync()
        {
            using (var client = new DaprClient())
            {
                // Example 1: Basic CRUD Operations
                await Example1_BasicCrudAsync(client);

                // Example 2: Strong Consistency
                await Example2_StrongConsistencyAsync(client);

                // Example 3: ETag Concurrency Control
                await Example3_ETagConcurrencyAsync(client);

                // Example 4: Bulk Operations
                await Example4_BulkOperationsAsync(client);

                // Example 5: State Transactions
                await Example5_StateTransactionsAsync(client);

                // Example 6: TTL Support
                await Example6_TtlSupportAsync(client);
            }
        }

        private static async Task Example1_BasicCrudAsync(DaprClient client)
        {
            WriteHeader("Example 1: Basic CRUD Operations");

            // Create an order
            var order = new OrderState
            {
                OrderId = "order-001",
                Status = "pending",
                Total = 99.99m,
                CreatedAt = DateTime.UtcNow
            };

            Console.WriteLine($"Saving order: {order.OrderId}");
            await client.SaveStateAsync(StoreName, order.OrderId, order);
            WriteSuccess("✓ Order saved");

            // Read the order
            Console.WriteLine($"Reading order: {order.OrderId}");
            var retrievedOrder = await client.GetStateAsync<OrderState>(StoreName, order.OrderId);
            WriteSuccess($"✓ Order retrieved: Status={retrievedOrder.Status}, Total=${retrievedOrder.Total}");

            // Update the order
            retrievedOrder.Status = "confirmed";
            Console.WriteLine($"Updating order status to: {retrievedOrder.Status}");
            await client.SaveStateAsync(StoreName, retrievedOrder.OrderId, retrievedOrder);
            WriteSuccess("✓ Order updated");

            // Delete the order
            Console.WriteLine($"Deleting order: {order.OrderId}");
            await client.DeleteStateAsync(StoreName, order.OrderId);
            WriteSuccess("✓ Order deleted");

            // Verify deletion
            var deletedOrder = await client.GetStateAsync<OrderState>(StoreName, order.OrderId);
            if (deletedOrder == null)
            {
                WriteSuccess("✓ Verified order was deleted (null returned)");
            }

            Console.WriteLine();
        }

        private static async Task Example2_StrongConsistencyAsync(DaprClient client)
        {
            WriteHeader("Example 2: Strong Consistency");

            var productId = "product-001";
            var inventory = new InventoryState
            {
                ProductId = productId,
                Quantity = 100
            };

            Console.WriteLine("Saving inventory with eventual consistency (default)");
            await client.SaveStateAsync(StoreName, productId, inventory);
            WriteSuccess("✓ Saved with eventual consistency");

            Console.WriteLine("Reading inventory with strong consistency");
            var retrieved = await client.GetStateAsync<InventoryState>(
                StoreName,
                productId,
                ConsistencyMode.Strong);
            WriteSuccess($"✓ Retrieved with strong consistency: Quantity={retrieved.Quantity}");

            // Cleanup
            await client.DeleteStateAsync(StoreName, productId);
            Console.WriteLine();
        }

        private static async Task Example3_ETagConcurrencyAsync(DaprClient client)
        {
            WriteHeader("Example 3: ETag Concurrency Control (Optimistic Locking)");

            var accountId = "account-001";
            var account = new AccountState
            {
                AccountId = accountId,
                Balance = 1000.00m
            };

            Console.WriteLine($"Creating account: {accountId} with balance ${account.Balance}");
            await client.SaveStateAsync(StoreName, accountId, account);
            WriteSuccess("✓ Account created");

            // Simulate concurrent access with first-write wins
            Console.WriteLine("\nSimulating concurrent updates with first-write wins:");

            // First update with ETag (will succeed)
            var options = new StateOptions
            {
                Concurrency = ConcurrencyMode.FirstWrite,
                ETag = "initial-etag"
            };

            account.Balance = 1100.00m;
            Console.WriteLine($"  Update 1: Attempting to set balance to ${account.Balance}");
            try
            {
                await client.SaveStateAsync(StoreName, accountId, account, options);
                WriteSuccess($"  ✓ Update 1 succeeded");
            }
            catch (DaprException ex)
            {
                WriteWarning($"  ✗ Update 1 failed (ETag mismatch): {ex.Message}");
            }

            // Cleanup
            await client.DeleteStateAsync(StoreName, accountId);
            Console.WriteLine();
        }

        private static async Task Example4_BulkOperationsAsync(DaprClient client)
        {
            WriteHeader("Example 4: Bulk Operations");

            // Bulk Save
            var items = new[]
            {
                new StateItem<InventoryState>
                {
                    Key = "product-101",
                    Value = new InventoryState { ProductId = "product-101", Quantity = 50 }
                },
                new StateItem<InventoryState>
                {
                    Key = "product-102",
                    Value = new InventoryState { ProductId = "product-102", Quantity = 75 }
                },
                new StateItem<InventoryState>
                {
                    Key = "product-103",
                    Value = new InventoryState { ProductId = "product-103", Quantity = 100 }
                }
            };

            Console.WriteLine($"Bulk saving {items.Length} inventory items");
            await client.SaveBulkStateAsync(StoreName, items);
            WriteSuccess($"✓ Saved {items.Length} items in bulk");

            // Bulk Get
            var keys = items.Select(i => i.Key).ToArray();
            Console.WriteLine($"\nBulk reading {keys.Length} inventory items (parallelism=5)");
            var retrieved = await client.GetBulkStateAsync<InventoryState>(StoreName, keys, parallelism: 5);
            var retrievedList = retrieved.ToList();
            WriteSuccess($"✓ Retrieved {retrievedList.Count} items in bulk");

            foreach (var item in retrievedList)
            {
                Console.WriteLine($"  - {item.Key}: Quantity={item.Value.Quantity}");
            }

            // Bulk Delete
            Console.WriteLine($"\nBulk deleting {keys.Length} inventory items");
            await client.DeleteBulkStateAsync(StoreName, keys);
            WriteSuccess($"✓ Deleted {keys.Length} items in bulk");

            Console.WriteLine();
        }

        private static async Task Example5_StateTransactionsAsync(DaprClient client)
        {
            WriteHeader("Example 5: State Transactions (Atomic Multi-Operation)");

            var fromAccount = "account-tx-001";
            var toAccount = "account-tx-002";

            // Setup: Create two accounts
            Console.WriteLine("Setting up accounts:");
            await client.SaveStateAsync(StoreName, fromAccount, new AccountState
            {
                AccountId = fromAccount,
                Balance = 1000.00m
            });
            Console.WriteLine($"  {fromAccount}: $1000");

            await client.SaveStateAsync(StoreName, toAccount, new AccountState
            {
                AccountId = toAccount,
                Balance = 500.00m
            });
            Console.WriteLine($"  {toAccount}: $500");

            WriteSuccess("✓ Accounts created");

            // Transfer $200 atomically
            var transferAmount = 200.00m;
            Console.WriteLine($"\nTransferring ${transferAmount} from {fromAccount} to {toAccount}");

            var fromBalance = (await client.GetStateAsync<AccountState>(StoreName, fromAccount)).Balance;
            var toBalance = (await client.GetStateAsync<AccountState>(StoreName, toAccount)).Balance;

            var operations = new[]
            {
                new StateTransactionRequest
                {
                    OperationType = StateOperationType.Upsert,
                    Key = fromAccount,
                    Value = new AccountState
                    {
                        AccountId = fromAccount,
                        Balance = fromBalance - transferAmount
                    }
                },
                new StateTransactionRequest
                {
                    OperationType = StateOperationType.Upsert,
                    Key = toAccount,
                    Value = new AccountState
                    {
                        AccountId = toAccount,
                        Balance = toBalance + transferAmount
                    }
                }
            };

            await client.ExecuteStateTransactionAsync(StoreName, operations);
            WriteSuccess("✓ Transaction completed atomically");

            // Verify final balances
            var fromFinal = await client.GetStateAsync<AccountState>(StoreName, fromAccount);
            var toFinal = await client.GetStateAsync<AccountState>(StoreName, toAccount);

            Console.WriteLine("\nFinal balances:");
            Console.WriteLine($"  {fromAccount}: ${fromFinal.Balance}");
            Console.WriteLine($"  {toAccount}: ${toFinal.Balance}");

            // Cleanup
            await client.DeleteBulkStateAsync(StoreName, new[] { fromAccount, toAccount });
            Console.WriteLine();
        }

        private static async Task Example6_TtlSupportAsync(DaprClient client)
        {
            WriteHeader("Example 6: TTL Support (Time-To-Live)");

            var sessionId = "session-001";
            var session = new SessionState
            {
                SessionId = sessionId,
                UserId = "user-123",
                CreatedAt = DateTime.UtcNow
            };

            var options = new StateOptions
            {
                TtlInSeconds = 60 // Expire after 60 seconds
            };

            Console.WriteLine($"Saving session with TTL: {options.TtlInSeconds} seconds");
            await client.SaveStateAsync(StoreName, sessionId, session, options);
            WriteSuccess($"✓ Session saved with {options.TtlInSeconds}s TTL");
            Console.WriteLine("  (Session will automatically expire after 60 seconds)");

            // Read it back immediately
            var retrieved = await client.GetStateAsync<SessionState>(StoreName, sessionId);
            if (retrieved != null)
            {
                WriteSuccess($"✓ Session retrieved immediately: UserId={retrieved.UserId}");
            }

            // Cleanup (don't wait for TTL in sample)
            await client.DeleteStateAsync(StoreName, sessionId);
            Console.WriteLine();
        }

        private static void WriteHeader(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n{message}");
            Console.WriteLine(new string('-', message.Length));
            Console.ResetColor();
        }

        private static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    // Sample data models

    public class OrderState
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InventoryState
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class AccountState
    {
        public string AccountId { get; set; }
        public decimal Balance { get; set; }
    }

    public class SessionState
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
