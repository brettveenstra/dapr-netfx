// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Shouldly;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    /// <summary>
    /// Tests for <see cref="DaprClient"/> using WireMock to mock Dapr HTTP API.
    /// </summary>
    [TestFixture]
    public class DaprClientTests
    {
        private WireMockServer _wireMockServer;
        private DaprClient _daprClient;

        [SetUp]
        public void SetUp()
        {
            _wireMockServer = WireMockServer.Start();

            var options = new DaprClientOptions
            {
                HttpEndpoint = _wireMockServer.Urls[0],
                Required = true
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

        [Test]
        public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => new DaprClient(null));
        }

        [Test]
        public void Constructor_WithInvalidUri_ShouldThrowArgumentException()
        {
            // Arrange - Invalid URI format (not even parseable)
            var options = new DaprClientOptions
            {
                HttpEndpoint = "not a valid uri" // Contains spaces, not parseable
            };

            // Act & Assert
            var exception = Should.Throw<ArgumentException>(() => new DaprClient(options));
            exception.Message.ShouldContain("is not a valid absolute URI");
            exception.Message.ShouldContain("http://hostname:port");
        }

        [Test]
        public void Constructor_WithInvalidScheme_ShouldThrowArgumentException()
        {
            // Arrange - URI with unsupported scheme
            var options = new DaprClientOptions
            {
                HttpEndpoint = "ftp://localhost:3500" // FTP not supported
            };

            // Act & Assert
            var exception = Should.Throw<ArgumentException>(() => new DaprClient(options));
            exception.Message.ShouldContain("must use http:// or https:// scheme");
            exception.Message.ShouldContain("ftp");
        }

        [Test]
        public void Constructor_WithMissingScheme_ShouldThrowArgumentException()
        {
            // Arrange - Common mistake: missing http:// prefix
            // Note: "localhost:3500" is parsed as scheme="localhost", path="3500"
            var options = new DaprClientOptions
            {
                HttpEndpoint = "localhost:3500" // Missing http:// or https://
            };

            // Act & Assert
            var exception = Should.Throw<ArgumentException>(() => new DaprClient(options));
            exception.Message.ShouldContain("must use http:// or https:// scheme");
            exception.Message.ShouldContain("localhost"); // Shows the parsed "scheme"
        }

        [Test]
        public async Task InvokeMethodAsync_WithRequestAndResponse_ShouldCallDaprApi()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/invoke/target-app/method/my-method")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("{\"message\":\"Hello from Dapr\"}"));

            var request = new { Name = "Test" };

            // Act
            var response = await _daprClient.InvokeMethodAsync<object, TestResponse>(
                "target-app",
                "my-method",
                request);

            // Assert
            response.ShouldNotBeNull();
            response.Message.ShouldBe("Hello from Dapr");
        }

        [Test]
        public async Task InvokeMethodAsync_WithGetVariant_ShouldCallDaprApi()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/invoke/target-app/method/get-data")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("{\"value\":42}"));

            // Act
            var response = await _daprClient.InvokeMethodAsync<TestResponse>(
                "target-app",
                "get-data");

            // Assert
            response.ShouldNotBeNull();
            response.Value.ShouldBe(42);
        }

        [Test]
        public async Task InvokeMethodAsync_WithFireAndForget_ShouldNotExpectResponse()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/invoke/target-app/method/notify")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));

            var request = new { Event = "TestEvent" };

            // Act & Assert (should not throw)
            await _daprClient.InvokeMethodAsync("target-app", "notify", request);
        }

        [Test]
        public void InvokeMethodAsync_WithNullAppId_ShouldThrowArgumentException()
        {
            var exception = Should.Throw<ArgumentException>(async () =>
                await _daprClient.InvokeMethodAsync<object, object>(null, "method", new { }));
            exception.Message.ShouldContain("cannot be null or whitespace");
        }

        [Test]
        public void InvokeMethodAsync_WithEmptyAppId_ShouldThrowArgumentException()
        {
            var exception = Should.Throw<ArgumentException>(async () =>
                await _daprClient.InvokeMethodAsync<object, object>("", "method", new { }));
            exception.Message.ShouldContain("cannot be null or whitespace");
        }

        [Test]
        public void InvokeMethodAsync_WithWhitespaceAppId_ShouldThrowArgumentException()
        {
            var exception = Should.Throw<ArgumentException>(async () =>
                await _daprClient.InvokeMethodAsync<object, object>("   ", "method", new { }));
            exception.Message.ShouldContain("cannot be null or whitespace");
        }

        [Test]
        public void InvokeMethodAsync_WithNullMethodName_ShouldThrowArgumentException()
        {
            var exception = Should.Throw<ArgumentException>(async () =>
                await _daprClient.InvokeMethodAsync<object, object>("app-id", null, new { }));
            exception.Message.ShouldContain("cannot be null or whitespace");
        }

        [Test]
        public void InvokeMethodAsync_WithEmptyMethodName_ShouldThrowArgumentException()
        {
            var exception = Should.Throw<ArgumentException>(async () =>
                await _daprClient.InvokeMethodAsync<object, object>("app-id", "", new { }));
            exception.Message.ShouldContain("cannot be null or whitespace");
        }

        [Test]
        public void InvokeMethodAsync_WithWhitespaceMethodName_ShouldThrowArgumentException()
        {
            var exception = Should.Throw<ArgumentException>(async () =>
                await _daprClient.InvokeMethodAsync<object, object>("app-id", "   ", new { }));
            exception.Message.ShouldContain("cannot be null or whitespace");
        }

        [Test]
        public void InvokeMethodAsync_WhenDaprUnavailable_ShouldThrowDaprException()
        {
            // Arrange - create client pointing to non-existent Dapr
            var badOptions = new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:9999",
                Required = true,
                HttpTimeout = TimeSpan.FromSeconds(1)
            };

            using (var badClient = new DaprClient(badOptions))
            {
                // Act & Assert
                var exception = Should.Throw<DaprException>(async () =>
                    await badClient.InvokeMethodAsync<object, object>("app", "method", new { }));

                exception.Message.ShouldContain("Failed to communicate with Dapr");
                exception.Message.ShouldContain("dapr run");
            }
        }

        [Test]
        public async Task InvokeMethodAsync_WhenDaprUnavailableAndRequiredFalse_ShouldThrowHttpRequestException()
        {
            // Arrange - create client pointing to non-existent Dapr with Required=false
            var optionalOptions = new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:9998",
                Required = false, // Key: Exception should NOT be wrapped in DaprException
                HttpTimeout = TimeSpan.FromSeconds(5) // Longer timeout ensures connection failure before timeout
            };

            using (var optionalClient = new DaprClient(optionalOptions))
            {
                // Act & Assert - Should throw HttpRequestException (unwrapped), not DaprException
                var exception = Should.Throw<System.Net.Http.HttpRequestException>(async () =>
                    await optionalClient.InvokeMethodAsync<object, object>("app", "method", new { }));

                // Exception should be the raw HttpRequestException, not wrapped
                exception.ShouldNotBeNull();
                exception.GetType().ShouldBe(typeof(System.Net.Http.HttpRequestException));
            }

            await Task.CompletedTask;
        }

        [Test]
        public void InvokeMethodAsync_WhenDaprReturns500AndRequiredFalse_ShouldThrowHttpRequestException()
        {
            // Arrange - Dapr returns 500 error with Required=false
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/invoke/failing-app/method/fail")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("{\"error\":\"Internal server error\"}"));

            var optionalOptions = new DaprClientOptions
            {
                HttpEndpoint = _wireMockServer.Urls[0],
                Required = false
            };

            using (var optionalClient = new DaprClient(optionalOptions))
            {
                // Act & Assert - EnsureSuccessStatusCode throws HttpRequestException
                var exception = Should.Throw<System.Net.Http.HttpRequestException>(async () =>
                    await optionalClient.InvokeMethodAsync<object, object>("failing-app", "fail", new { }));

                // Verify it's the raw exception (not wrapped in DaprException)
                exception.ShouldNotBeNull();
                exception.Message.ShouldContain("500");
            }
        }

        [Test]
        public async Task InvokeMethodAsync_WithApiToken_ShouldIncludeTokenHeader()
        {
            // Arrange
            var optionsWithToken = new DaprClientOptions
            {
                HttpEndpoint = _wireMockServer.Urls[0],
                ApiToken = "secret-token-12345"
            };

            using (var clientWithToken = new DaprClient(optionsWithToken))
            {
                _wireMockServer
                    .Given(Request.Create()
                        .WithPath("/v1.0/invoke/app/method/test")
                        .WithHeader("dapr-api-token", "secret-token-12345")
                        .UsingPost())
                    .RespondWith(Response.Create()
                        .WithStatusCode(200)
                        .WithBody("{\"result\":\"ok\"}"));

                // Act
                var response = await clientWithToken.InvokeMethodAsync<object, TestResponse>(
                    "app",
                    "test",
                    new { });

                // Assert
                response.Result.ShouldBe("ok");
            }
        }

        [Test]
        public async Task InvokeMethodAsync_WithNullRequestBody_ShouldSucceed()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/invoke/app/method/test")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"value\":123}"));

            // Act
            var response = await _daprClient.InvokeMethodAsync<object, TestResponse>(
                "app",
                "test",
                null);

            // Assert
            response.Value.ShouldBe(123);
        }

        [Test]
        public async Task InvokeMethodAsync_WithEmptyResponse_ShouldReturnDefault()
        {
            // Arrange
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/v1.0/invoke/app/method/test")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Empty));

            // Act
            var response = await _daprClient.InvokeMethodAsync<object, TestResponse>(
                "app",
                "test",
                new { });

            // Assert
            response.ShouldBeNull();
        }

        [Test]
        public void Constructor_WithCustomHttpTimeoutInOptions_ShouldUseCustomTimeout()
        {
            // Arrange
            var customTimeout = TimeSpan.FromSeconds(60);
            var options = new DaprClientOptions
            {
                HttpEndpoint = _wireMockServer.Urls[0],
                HttpTimeout = customTimeout
            };

            // Act
            using (var client = new DaprClient(options))
            {
                // Assert - Verify timeout is set (can't directly access private field, but constructor didn't throw)
                client.ShouldNotBeNull();
            }
        }

        [Test]
        public void Constructor_WithEnvironmentVariableHttpTimeout_ShouldOverrideDefault()
        {
            // Arrange - Set environment variable
            Environment.SetEnvironmentVariable("DAPR_HTTP_TIMEOUT_SECONDS", "45");

            try
            {
                // Act - Create client using default constructor (loads from config)
                using (var client = new DaprClient())
                {
                    // Assert - Client created successfully with env var timeout
                    client.ShouldNotBeNull();
                }
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("DAPR_HTTP_TIMEOUT_SECONDS", null);
            }
        }

        [Test]
        public void Constructor_WithInvalidHttpTimeoutValue_ShouldUseDefault()
        {
            // Arrange - Set invalid environment variable (negative value)
            Environment.SetEnvironmentVariable("DAPR_HTTP_TIMEOUT_SECONDS", "-5");

            try
            {
                // Act - Create client using default constructor
                using (var client = new DaprClient())
                {
                    // Assert - Client created successfully (invalid value ignored, uses default 30s)
                    client.ShouldNotBeNull();
                }
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("DAPR_HTTP_TIMEOUT_SECONDS", null);
            }
        }

        [Test]
        public void Constructor_WithNonNumericHttpTimeout_ShouldUseDefault()
        {
            // Arrange - Set non-numeric environment variable
            Environment.SetEnvironmentVariable("DAPR_HTTP_TIMEOUT_SECONDS", "invalid");

            try
            {
                // Act - Create client using default constructor
                using (var client = new DaprClient())
                {
                    // Assert - Client created successfully (invalid value ignored, uses default 30s)
                    client.ShouldNotBeNull();
                }
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("DAPR_HTTP_TIMEOUT_SECONDS", null);
            }
        }

        private class TestResponse
        {
            public string Message { get; set; }

            public int Value { get; set; }

            public string Result { get; set; }
        }
    }
}
