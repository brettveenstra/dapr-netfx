// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Http
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// HTTP client for Dapr API communication using singleton pattern for .NET Framework.
    /// </summary>
    internal class DaprHttpClient : IDisposable
    {
        private static readonly HttpClient SharedHttpClient;
        private readonly DaprClientOptions _options;
        private bool _disposed;

        static DaprHttpClient()
        {
            // .NET Framework best practice: Configure ServicePointManager for connection pooling
            ServicePointManager.DefaultConnectionLimit = 50;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            // DNS refresh: Recycle connections every 2 minutes to pick up DNS changes
            // Critical for Azure/Kubernetes where Dapr endpoint IPs change during scaling/deployments
            // Without this, singleton HttpClient caches DNS indefinitely causing "Dapr unavailable" errors
            ServicePointManager.DnsRefreshTimeout = 120000; // 2 minutes in milliseconds

            // Singleton HttpClient (no HttpClientFactory in .NET Framework)
            SharedHttpClient = new HttpClient();
            SharedHttpClient.DefaultRequestHeaders.Add("User-Agent", "DaprNetFx/0.1.0-alpha");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprHttpClient"/> class.
        /// </summary>
        /// <param name="options">The Dapr client options.</param>
        public DaprHttpClient(DaprClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            // Validate HTTP endpoint format at construction time for fail-fast behavior
            // Better to get clear error on startup than cryptic exception on first API call
            if (!Uri.TryCreate(_options.HttpEndpoint, UriKind.Absolute, out var endpointUri))
            {
                throw new ArgumentException(
                    $"HttpEndpoint '{_options.HttpEndpoint}' is not a valid absolute URI. " +
                    $"Expected format: http://hostname:port or https://hostname:port",
                    nameof(options));
            }

            if (endpointUri.Scheme != Uri.UriSchemeHttp && endpointUri.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException(
                    $"HttpEndpoint '{_options.HttpEndpoint}' must use http:// or https:// scheme. " +
                    $"Found scheme: {endpointUri.Scheme}",
                    nameof(options));
            }

            // Configure ServicePoint for the Dapr endpoint to enable connection recycling
            // Defense-in-depth: Even if DNS returns same IP, recycling connections helps
            // recover from transient network issues (Azure load balancer failures, etc.)
            var servicePoint = ServicePointManager.FindServicePoint(endpointUri);
            servicePoint.ConnectionLeaseTimeout = 120000; // 2 minutes
            servicePoint.ConnectionLimit = 50; // Max concurrent connections to this endpoint

            // Note: Cannot set timeout on shared HttpClient (would affect all instances)
            // Timeout is enforced per-request using cancellation tokens
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Note: Do NOT dispose SharedHttpClient (singleton pattern)
                _disposed = true;
            }
        }

        /// <summary>
        /// Sends a POST request with JSON body to the specified path.
        /// </summary>
        /// <typeparam name="TRequest">The request body type.</typeparam>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="path">The API path (e.g., "/v1.0/invoke/app-id/method/method-name").</param>
        /// <param name="requestBody">The request body.</param>
        /// <returns>The deserialized response.</returns>
        internal async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
            string path,
            TRequest requestBody)
        {
            var requestUri = BuildUri(path);
            var request = CreateRequest(HttpMethod.Post, requestUri);

            if (requestBody != null)
            {
                var json = JsonConvert.SerializeObject(requestBody);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var response = await SendRequestAsync(request).ConfigureAwait(false))
            {
                return await DeserializeResponseAsync<TResponse>(response).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a GET request to the specified path.
        /// </summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="path">The API path.</param>
        /// <returns>The deserialized response.</returns>
        internal async Task<TResponse> GetJsonAsync<TResponse>(string path)
        {
            var requestUri = BuildUri(path);
            var request = CreateRequest(HttpMethod.Get, requestUri);

            using (var response = await SendRequestAsync(request).ConfigureAwait(false))
            {
                return await DeserializeResponseAsync<TResponse>(response).ConfigureAwait(false);
            }
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri uri)
        {
            var request = new HttpRequestMessage(method, uri);

            // Add Dapr API token if configured
            if (!string.IsNullOrWhiteSpace(_options.ApiToken))
            {
                request.Headers.Add("dapr-api-token", _options.ApiToken);
            }

            return request;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            try
            {
                // Use cancellation token for per-request timeout (singleton HttpClient pattern)
                using (var cts = new System.Threading.CancellationTokenSource(_options.HttpTimeout))
                {
                    response = await SharedHttpClient.SendAsync(request, cts.Token)
                        .ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    return response; // Caller must dispose
                }
            }
            catch (HttpRequestException ex) when (_options.Required)
            {
                response?.Dispose();
                throw new DaprException(
                    $"Failed to communicate with Dapr at {_options.HttpEndpoint}. " +
                    $"Ensure Dapr sidecar is running. " +
                    $"For local development, run: dapr run --app-id <your-app-id> " +
                    $"To disable this check, set Dapr:Required=false in app.config.",
                    ex);
            }
            catch (System.Threading.Tasks.TaskCanceledException ex) when (_options.Required)
            {
                response?.Dispose();
                throw new DaprException(
                    $"Request to Dapr at {_options.HttpEndpoint} timed out after {_options.HttpTimeout.TotalSeconds}s. " +
                    $"Ensure Dapr sidecar is running and responsive. " +
                    $"To disable this check, set Dapr:Required=false in app.config.",
                    ex);
            }
            catch
            {
                // Dispose response on any other exception (e.g., EnsureSuccessStatusCode with Required=false)
                response?.Dispose();
                throw;
            }
        }

        private async Task<TResponse> DeserializeResponseAsync<TResponse>(
            HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
            {
                return default(TResponse);
            }

            return JsonConvert.DeserializeObject<TResponse>(json);
        }

        private Uri BuildUri(string path)
        {
            var baseUri = _options.HttpEndpoint.TrimEnd('/');
            var fullPath = path.StartsWith("/") ? path : "/" + path;
            return new Uri(baseUri + fullPath);
        }
    }
}
