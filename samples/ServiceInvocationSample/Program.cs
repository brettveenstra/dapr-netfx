// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Samples.ServiceInvocation
{
    using System;
    using System.Threading.Tasks;
    using DaprNetFx;

    /// <summary>
    /// Sample demonstrating Dapr service invocation from .NET Framework 4.8.
    /// </summary>
    /// <remarks>
    /// Prerequisites:
    /// 1. Install Dapr CLI: https://docs.dapr.io/getting-started/install-dapr-cli/
    /// 2. Initialize Dapr: dapr init
    /// 3. Start target service: dapr run --app-id order-processor --app-port 5001 --dapr-http-port 3501
    /// 4. Run this sample: dapr run --app-id service-invocation-sample --dapr-http-port 3500 -- ServiceInvocationSample.exe
    /// </remarks>
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("=== DaprNetFx Service Invocation Sample ===");
            Console.WriteLine();

            try
            {
                RunSampleAsync().GetAwaiter().GetResult();
            }
            catch (DaprException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Dapr Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Make sure:");
                Console.WriteLine("1. Dapr sidecar is running (dapr run ...)");
                Console.WriteLine("2. Target service is available");
                Console.WriteLine("3. HTTP endpoint is correct (default: http://localhost:3500)");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                Environment.Exit(1);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task RunSampleAsync()
        {
            // Example 1: Default configuration (assumes Dapr sidecar on localhost:3500)
            Console.WriteLine("Example 1: Default Configuration");
            Console.WriteLine("----------------------------------");
            await InvokeWithDefaultConfigAsync();

            Console.WriteLine();

            // Example 2: Custom configuration
            Console.WriteLine("Example 2: Custom Configuration");
            Console.WriteLine("--------------------------------");
            await InvokeWithCustomConfigAsync();

            Console.WriteLine();

            // Example 3: Optional Dapr (won't throw if sidecar unavailable)
            Console.WriteLine("Example 3: Optional Dapr Mode");
            Console.WriteLine("-----------------------------");
            await InvokeWithOptionalDaprAsync();
        }

        /// <summary>
        /// Example 1: Use default Dapr configuration.
        /// </summary>
        private static async Task InvokeWithDefaultConfigAsync()
        {
            using (var daprClient = new DaprClient())
            {
                Console.WriteLine("Using default Dapr configuration (localhost:3500)");
                Console.WriteLine();

                // Invoke another service's method
                var request = new OrderRequest
                {
                    OrderId = "ORDER-001",
                    ProductName = "Widget",
                    Quantity = 5,
                };

                Console.WriteLine($"Invoking 'order-processor' service, method 'process'...");
                Console.WriteLine($"Request: {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");

                try
                {
                    var response = await daprClient.InvokeMethodAsync<OrderRequest, OrderResponse>(
                        appId: "order-processor",
                        methodName: "process",
                        request: request);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Success! Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                    Console.ResetColor();
                }
                catch (DaprException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Service not available: {ex.Message}");
                    Console.WriteLine("(This is expected if target service isn't running)");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Example 2: Use custom Dapr configuration.
        /// </summary>
        private static async Task InvokeWithCustomConfigAsync()
        {
            var options = new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                ApiToken = null, // Set if Dapr API token authentication is enabled
                HttpTimeout = TimeSpan.FromSeconds(10),
                Required = true,
            };

            using (var daprClient = new DaprClient(options))
            {
                Console.WriteLine($"Custom Dapr HTTP Endpoint: {options.HttpEndpoint}");
                Console.WriteLine($"Custom Timeout: {options.HttpTimeout.TotalSeconds}s");
                Console.WriteLine();

                // Simple GET-style invocation (no request body)
                Console.WriteLine("Invoking 'order-processor' service, method 'health'...");

                try
                {
                    var response = await daprClient.InvokeMethodAsync<HealthResponse>(
                        appId: "order-processor",
                        methodName: "health");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Health check: {response?.Status ?? "OK"}");
                    Console.ResetColor();
                }
                catch (DaprException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Health check failed: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Example 3: Optional Dapr mode - won't throw if sidecar is unavailable.
        /// </summary>
        private static async Task InvokeWithOptionalDaprAsync()
        {
            var options = new DaprClientOptions
            {
                Required = false, // Application can run without Dapr
            };

            using (var daprClient = new DaprClient(options))
            {
                Console.WriteLine("Dapr Required: false (degraded mode if unavailable)");
                Console.WriteLine();

                try
                {
                    var response = await daprClient.InvokeMethodAsync<string>(
                        appId: "order-processor",
                        methodName: "status");

                    Console.WriteLine($"Status: {response}");
                }
                catch (DaprException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Dapr unavailable - application can continue in degraded mode");
                    Console.WriteLine($"Details: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
    }

    // Sample data models
    internal class OrderRequest
    {
        public string OrderId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }
    }

    internal class OrderResponse
    {
        public string OrderId { get; set; }

        public string Status { get; set; }

        public decimal TotalPrice { get; set; }
    }

    internal class HealthResponse
    {
        public string Status { get; set; }
    }
}
