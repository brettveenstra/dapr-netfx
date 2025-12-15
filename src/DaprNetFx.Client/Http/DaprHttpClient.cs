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

            var response = await SendRequestAsync(request).ConfigureAwait(false);
            return await DeserializeResponseAsync<TResponse>(response).ConfigureAwait(false);
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

            var response = await SendRequestAsync(request).ConfigureAwait(false);
            return await DeserializeResponseAsync<TResponse>(response).ConfigureAwait(false);
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
            try
            {
                // Use cancellation token for per-request timeout (singleton HttpClient pattern)
                using (var cts = new System.Threading.CancellationTokenSource(_options.HttpTimeout))
                {
                    var response = await SharedHttpClient.SendAsync(request, cts.Token)
                        .ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    return response;
                }
            }
            catch (Exception ex) when (
                (ex is HttpRequestException || ex is System.Threading.Tasks.TaskCanceledException)
                && _options.Required)
            {
                throw new DaprException(
                    $"Failed to communicate with Dapr at {_options.HttpEndpoint}. " +
                    $"Ensure Dapr sidecar is running. " +
                    $"For local development, run: dapr run --app-id <your-app-id> " +
                    $"To disable this check, set Dapr:Required=false in app.config.",
                    ex);
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
