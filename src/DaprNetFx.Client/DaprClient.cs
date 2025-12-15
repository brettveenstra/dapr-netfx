// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    using System;
    using System.Threading.Tasks;
    using DaprNetFx.Configuration;
    using DaprNetFx.Http;

    /// <summary>
    /// Client for interacting with Dapr.
    /// </summary>
    public class DaprClient : IDisposable
    {
        private readonly DaprHttpClient _httpClient;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprClient"/> class with default options.
        /// </summary>
        /// <remarks>
        /// Configuration is loaded from app.config/web.config and environment variables.
        /// Precedence: Environment variables > app.config > defaults.
        /// </remarks>
        public DaprClient()
            : this(DaprConfigurationProvider.LoadOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprClient"/> class with custom options.
        /// </summary>
        /// <param name="options">The Dapr client options.</param>
        public DaprClient(DaprClientOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _httpClient = new DaprHttpClient(options);
        }

        /// <summary>
        /// Invokes a method on a remote Dapr application.
        /// </summary>
        /// <typeparam name="TRequest">The request body type.</typeparam>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="appId">The target application ID.</param>
        /// <param name="methodName">The method name to invoke.</param>
        /// <param name="request">The request body.</param>
        /// <returns>A task representing the asynchronous operation with the response.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="appId"/> or <paramref name="methodName"/> is null.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task<TResponse> InvokeMethodAsync<TRequest, TResponse>(
            string appId,
            string methodName,
            TRequest request)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            var path = $"/v1.0/invoke/{appId}/method/{methodName}";
            return await _httpClient.PostJsonAsync<TRequest, TResponse>(path, request)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes a method on a remote Dapr application without a request body.
        /// </summary>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="appId">The target application ID.</param>
        /// <param name="methodName">The method name to invoke.</param>
        /// <returns>A task representing the asynchronous operation with the response.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="appId"/> or <paramref name="methodName"/> is null.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task<TResponse> InvokeMethodAsync<TResponse>(
            string appId,
            string methodName)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            var path = $"/v1.0/invoke/{appId}/method/{methodName}";
            return await _httpClient.GetJsonAsync<TResponse>(path).ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes a method on a remote Dapr application without expecting a response.
        /// </summary>
        /// <typeparam name="TRequest">The request body type.</typeparam>
        /// <param name="appId">The target application ID.</param>
        /// <param name="methodName">The method name to invoke.</param>
        /// <param name="request">The request body.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="appId"/> or <paramref name="methodName"/> is null.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task InvokeMethodAsync<TRequest>(
            string appId,
            string methodName,
            TRequest request)
        {
            await InvokeMethodAsync<TRequest, object>(appId, methodName, request)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
