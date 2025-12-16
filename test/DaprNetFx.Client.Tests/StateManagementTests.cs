// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Client.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Shouldly;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    /// <summary>
    /// Tests for Dapr State Management operations.
    /// </summary>
    [TestFixture]
    public class StateManagementTests
    {
        private WireMockServer _wireMockServer;
        private DaprClient _daprClient;

        [SetUp]
        public void SetUp()
        {
            _wireMockServer = WireMockServer.Start();

            var options = new DaprClientOptions
            {
                HttpEndpoint = _wireMockServer.Url,
                Required = true,
            };

            _daprClient = new DaprClient(options);
        }

        [TearDown]
        public void TearDown()
        {
            _daprClient?.Dispose();
            _wireMockServer?.Stop();
            _wireMockServer?.Dispose();
        }

        #region SaveStateAsync Validation Tests

        [Test]
        public void SaveStateAsync_WithNullStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.SaveStateAsync<string>(null, "key1", "value1"));
        }

        [Test]
        public void SaveStateAsync_WithEmptyStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.SaveStateAsync<string>(string.Empty, "key1", "value1"));
        }

        [Test]
        public void SaveStateAsync_WithWhitespaceStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.SaveStateAsync<string>("  ", "key1", "value1"));
        }

        [Test]
        public void SaveStateAsync_WithNullKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.SaveStateAsync<string>("mystore", null, "value1"));
        }

        [Test]
        public void SaveStateAsync_WithEmptyKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.SaveStateAsync<string>("mystore", string.Empty, "value1"));
        }

        [Test]
        public void SaveStateAsync_WithWhitespaceKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.SaveStateAsync<string>("mystore", "   ", "value1"));
        }

        #endregion

        #region GetStateAsync Validation Tests

        [Test]
        public void GetStateAsync_WithNullStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.GetStateAsync<string>(null, "key1"));
        }

        [Test]
        public void GetStateAsync_WithEmptyStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.GetStateAsync<string>(string.Empty, "key1"));
        }

        [Test]
        public void GetStateAsync_WithWhitespaceStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.GetStateAsync<string>("   ", "key1"));
        }

        [Test]
        public void GetStateAsync_WithNullKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.GetStateAsync<string>("mystore", null));
        }

        [Test]
        public void GetStateAsync_WithEmptyKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.GetStateAsync<string>("mystore", string.Empty));
        }

        [Test]
        public void GetStateAsync_WithWhitespaceKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.GetStateAsync<string>("mystore", "  "));
        }

        #endregion

        #region DeleteStateAsync Validation Tests

        [Test]
        public void DeleteStateAsync_WithNullStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.DeleteStateAsync(null, "key1"));
        }

        [Test]
        public void DeleteStateAsync_WithEmptyStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.DeleteStateAsync(string.Empty, "key1"));
        }

        [Test]
        public void DeleteStateAsync_WithWhitespaceStoreName_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.DeleteStateAsync("  ", "key1"));
        }

        [Test]
        public void DeleteStateAsync_WithNullKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.DeleteStateAsync("mystore", null));
        }

        [Test]
        public void DeleteStateAsync_WithEmptyKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.DeleteStateAsync("mystore", string.Empty));
        }

        [Test]
        public void DeleteStateAsync_WithWhitespaceKey_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.DeleteStateAsync("mystore", "   "));
        }

        #endregion

        #region SaveStateAsync Happy Path Tests

        [Test]
        public async Task SaveStateAsync_WithValidInput_ShouldSucceed()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.SaveStateAsync("mystore", "key1", "value1");

            // Assert - no exception thrown
        }

        [Test]
        public async Task SaveStateAsync_WithOptions_ShouldIncludeOptionsInRequest()
        {
            // Arrange
            var options = new StateOptions
            {
                Concurrency = ConcurrencyMode.FirstWrite,
                Consistency = ConsistencyMode.Strong,
                ETag = "etag123",
                TtlInSeconds = 3600,
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.SaveStateAsync("mystore", "key1", "value1", options);

            // Assert
            var requests = _wireMockServer.LogEntries;
            requests.ShouldNotBeEmpty();

            var requestBody = requests.First().RequestMessage.Body;
            requestBody.ShouldContain("first-write");
            requestBody.ShouldContain("strong");
            requestBody.ShouldContain("etag123");
            requestBody.ShouldContain("ttlInSeconds");
        }

        #endregion

        #region GetStateAsync Happy Path Tests

        [Test]
        public async Task GetStateAsync_WhenKeyExists_ShouldReturnValue()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/key1")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("\"myvalue\""));

            // Act
            var result = await _daprClient.GetStateAsync<string>("mystore", "key1");

            // Assert
            result.ShouldBe("myvalue");
        }

        [Test]
        public async Task GetStateAsync_WhenKeyDoesNotExist_ShouldReturnDefault()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/nonexistent")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(204)); // 204 No Content for missing keys

            // Act
            var result = await _daprClient.GetStateAsync<string>("mystore", "nonexistent");

            // Assert
            result.ShouldBeNull(); // default(string) is null
        }

        [Test]
        public async Task GetStateAsync_WithStrongConsistency_ShouldIncludeQueryParam()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/key1")
                    .WithParam("consistency", "strong")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("\"value1\""));

            // Act
            var result = await _daprClient.GetStateAsync<string>("mystore", "key1", ConsistencyMode.Strong);

            // Assert
            result.ShouldBe("value1");
        }

        #endregion

        #region DeleteStateAsync Happy Path Tests

        [Test]
        public async Task DeleteStateAsync_WithValidInput_ShouldSucceed()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/key1")
                    .UsingDelete())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.DeleteStateAsync("mystore", "key1");

            // Assert - no exception thrown
        }

        [Test]
        public async Task DeleteStateAsync_WithOptions_ShouldIncludeQueryParams()
        {
            // Arrange
            var options = new StateOptions
            {
                Concurrency = ConcurrencyMode.FirstWrite,
                Consistency = ConsistencyMode.Strong,
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/key1")
                    .WithParam("concurrency", "first-write")
                    .WithParam("consistency", "strong")
                    .UsingDelete())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.DeleteStateAsync("mystore", "key1", options);

            // Assert - no exception thrown
        }

        #endregion

        #region Error Scenario Tests

        [Test]
        public void SaveStateAsync_WhenDaprUnavailableAndRequired_ShouldThrowDaprException()
        {
            // Arrange - WireMock not configured, will return 404

            // Act & Assert
            Should.Throw<DaprException>(async () =>
                await _daprClient.SaveStateAsync("mystore", "key1", "value1"));
        }

        [Test]
        public void GetStateAsync_WhenDaprUnavailableAndRequired_ShouldThrowDaprException()
        {
            // Arrange - WireMock not configured, will return 404

            // Act & Assert
            Should.Throw<DaprException>(async () =>
                await _daprClient.GetStateAsync<string>("mystore", "key1"));
        }

        [Test]
        public void DeleteStateAsync_WhenDaprUnavailableAndRequired_ShouldThrowDaprException()
        {
            // Arrange - WireMock not configured, will return 404

            // Act & Assert
            Should.Throw<DaprException>(async () =>
                await _daprClient.DeleteStateAsync("mystore", "key1"));
        }

        [Test]
        public void SaveStateAsync_WhenDaprUnavailableAndNotRequired_ShouldThrowHttpRequestException()
        {
            // Arrange
            var options = new DaprClientOptions
            {
                HttpEndpoint = _wireMockServer.Url,
                Required = false,
            };

            using (var client = new DaprClient(options))
            {
                // Act & Assert - should throw HttpRequestException (not wrapped in DaprException)
                var exception = Should.Throw<System.Net.Http.HttpRequestException>(async () =>
                    await client.SaveStateAsync("mystore", "key1", "value1"));

                // Verify it's the raw exception, not wrapped
                exception.ShouldNotBeNull();
                exception.GetType().ShouldBe(typeof(System.Net.Http.HttpRequestException));
            }
        }

        #endregion

        #region Bulk Operations Validation Tests

        [Test]
        public void SaveBulkStateAsync_WithNullItems_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.SaveBulkStateAsync<string>("mystore", null));
        }

        [Test]
        public void SaveBulkStateAsync_WithEmptyItems_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.SaveBulkStateAsync<string>("mystore", new StateItem<string>[0]));
        }

        [Test]
        public void GetBulkStateAsync_WithNullKeys_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.GetBulkStateAsync<string>("mystore", null));
        }

        [Test]
        public void GetBulkStateAsync_WithEmptyKeys_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.GetBulkStateAsync<string>("mystore", new string[0]));
        }

        [Test]
        public void DeleteBulkStateAsync_WithNullKeys_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.DeleteBulkStateAsync("mystore", null));
        }

        [Test]
        public void DeleteBulkStateAsync_WithEmptyKeys_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.DeleteBulkStateAsync("mystore", new string[0]));
        }

        #endregion

        #region Bulk Operations Happy Path Tests

        [Test]
        public async Task SaveBulkStateAsync_WithValidItems_ShouldSucceed()
        {
            // Arrange
            var items = new[]
            {
                new StateItem<string> { Key = "key1", Value = "value1" },
                new StateItem<string> { Key = "key2", Value = "value2" },
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.SaveBulkStateAsync("mystore", items);

            // Assert - no exception thrown
        }

        [Test]
        public async Task GetBulkStateAsync_WithValidKeys_ShouldReturnItems()
        {
            // Arrange
            var keys = new[] { "key1", "key2" };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/bulk")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"[
                        { ""key"": ""key1"", ""data"": ""value1"", ""etag"": ""1"" },
                        { ""key"": ""key2"", ""data"": ""value2"", ""etag"": ""2"" }
                    ]"));

            // Act
            var result = await _daprClient.GetBulkStateAsync<string>("mystore", keys);

            // Assert
            var resultList = result.ToList();
            resultList.Count.ShouldBe(2);
            resultList[0].Key.ShouldBe("key1");
            resultList[0].Value.ShouldBe("value1");
            resultList[1].Key.ShouldBe("key2");
            resultList[1].Value.ShouldBe("value2");
        }

        [Test]
        public async Task GetBulkStateAsync_WithParallelism_ShouldIncludeInRequest()
        {
            // Arrange
            var keys = new[] { "key1", "key2", "key3" };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/bulk")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("[]"));

            // Act
            await _daprClient.GetBulkStateAsync<string>("mystore", keys, parallelism: 5);

            // Assert
            var requests = _wireMockServer.LogEntries;
            requests.ShouldNotBeEmpty();

            var requestBody = requests.First().RequestMessage.Body;
            requestBody.ShouldContain("\"parallelism\":5");
        }

        [Test]
        public async Task DeleteBulkStateAsync_WithValidKeys_ShouldSucceed()
        {
            // Arrange
            var keys = new[] { "key1", "key2" };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.DeleteBulkStateAsync("mystore", keys);

            // Assert - no exception thrown
        }

        #endregion

        #region State Transaction Tests

        [Test]
        public void ExecuteStateTransactionAsync_WithNullOperations_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.ExecuteStateTransactionAsync("mystore", null));
        }

        [Test]
        public void ExecuteStateTransactionAsync_WithEmptyOperations_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(async () =>
                await _daprClient.ExecuteStateTransactionAsync("mystore", new StateTransactionRequest[0]));
        }

        [Test]
        public async Task ExecuteStateTransactionAsync_WithUpsertOperation_ShouldSucceed()
        {
            // Arrange
            var operations = new[]
            {
                new StateTransactionRequest
                {
                    OperationType = StateOperationType.Upsert,
                    Key = "key1",
                    Value = "value1",
                },
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/transaction")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.ExecuteStateTransactionAsync("mystore", operations);

            // Assert - no exception thrown
        }

        [Test]
        public async Task ExecuteStateTransactionAsync_WithDeleteOperation_ShouldSucceed()
        {
            // Arrange
            var operations = new[]
            {
                new StateTransactionRequest
                {
                    OperationType = StateOperationType.Delete,
                    Key = "key1",
                },
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/transaction")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.ExecuteStateTransactionAsync("mystore", operations);

            // Assert - no exception thrown
        }

        [Test]
        public async Task ExecuteStateTransactionAsync_WithMixedOperations_ShouldSucceed()
        {
            // Arrange
            var operations = new[]
            {
                new StateTransactionRequest
                {
                    OperationType = StateOperationType.Upsert,
                    Key = "key1",
                    Value = "value1",
                },
                new StateTransactionRequest
                {
                    OperationType = StateOperationType.Delete,
                    Key = "key2",
                },
                new StateTransactionRequest
                {
                    OperationType = StateOperationType.Upsert,
                    Key = "key3",
                    Value = "value3",
                    ETag = "etag123",
                },
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/transaction")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(204));

            // Act
            await _daprClient.ExecuteStateTransactionAsync("mystore", operations);

            // Assert
            var requests = _wireMockServer.LogEntries;
            requests.ShouldNotBeEmpty();

            var requestBody = requests.First().RequestMessage.Body;
            requestBody.ShouldContain("upsert");
            requestBody.ShouldContain("delete");
            requestBody.ShouldContain("key1");
            requestBody.ShouldContain("key2");
            requestBody.ShouldContain("key3");
        }

        [Test]
        public void ExecuteStateTransactionAsync_WhenStoreDoesNotSupportTransactions_ShouldThrowDaprException()
        {
            // Arrange
            var operations = new[]
            {
                new StateTransactionRequest
                {
                    OperationType = StateOperationType.Upsert,
                    Key = "key1",
                    Value = "value1",
                },
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/state/mystore/transaction")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("state store does not support transactions"));

            // Act & Assert
            Should.Throw<DaprException>(async () =>
                await _daprClient.ExecuteStateTransactionAsync("mystore", operations));
        }

        #endregion
    }
}
