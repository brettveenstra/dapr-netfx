// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DaprNetFx.Configuration;
    using DaprNetFx.Http;
    using DaprNetFx.Http.Models;
    using Newtonsoft.Json;

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
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="appId"/> or <paramref name="methodName"/> is null or whitespace.
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
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(methodName));
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
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="appId"/> or <paramref name="methodName"/> is null or whitespace.
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
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(methodName));
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

        /// <summary>
        /// Saves state for a specific key in the state store.
        /// </summary>
        /// <typeparam name="TValue">The state value type.</typeparam>
        /// <param name="storeName">The name of the state store.</param>
        /// <param name="key">The state key.</param>
        /// <param name="value">The state value to save.</param>
        /// <param name="options">Optional state options (concurrency, consistency, ETag, TTL).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="storeName"/> or <paramref name="key"/> is null or whitespace.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task SaveStateAsync<TValue>(
            string storeName,
            string key,
            TValue value,
            StateOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(storeName));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            var stateRequest = new SaveStateRequest
            {
                Key = key,
                Value = value,
                ETag = options?.ETag,
            };

            if (options?.Concurrency != null || options?.Consistency != null)
            {
                stateRequest.Options = new StateOperationOptions
                {
                    Concurrency = options.Concurrency.HasValue
                        ? options.Concurrency.Value == ConcurrencyMode.FirstWrite ? "first-write" : "last-write"
                        : null,
                    Consistency = options.Consistency.HasValue
                        ? options.Consistency.Value == ConsistencyMode.Strong ? "strong" : "eventual"
                        : null,
                };
            }

            if (options?.TtlInSeconds != null)
            {
                stateRequest.Metadata = new Dictionary<string, string>
                {
                    { "ttlInSeconds", options.TtlInSeconds.Value.ToString() },
                };
            }

            var path = $"/v1.0/state/{storeName}";
            var requestArray = new[] { stateRequest };
            await _httpClient.PostJsonNoContentAsync(path, requestArray).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets state for a specific key from the state store.
        /// </summary>
        /// <typeparam name="TValue">The state value type.</typeparam>
        /// <param name="storeName">The name of the state store.</param>
        /// <param name="key">The state key.</param>
        /// <param name="consistency">Optional consistency mode (strong or eventual).</param>
        /// <returns>
        /// A task representing the asynchronous operation with the state value.
        /// Returns default(TValue) if the key does not exist.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="storeName"/> or <paramref name="key"/> is null or whitespace.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task<TValue> GetStateAsync<TValue>(
            string storeName,
            string key,
            ConsistencyMode? consistency = null)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(storeName));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            var path = $"/v1.0/state/{storeName}/{key}";
            Dictionary<string, string> queryParams = null;

            if (consistency.HasValue)
            {
                queryParams = new Dictionary<string, string>
                {
                    { "consistency", consistency.Value == ConsistencyMode.Strong ? "strong" : "eventual" },
                };
            }

            return await _httpClient.GetJsonAsync<TValue>(path, queryParams).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes state for a specific key from the state store.
        /// </summary>
        /// <param name="storeName">The name of the state store.</param>
        /// <param name="key">The state key to delete.</param>
        /// <param name="options">Optional state options (concurrency, consistency, ETag).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="storeName"/> or <paramref name="key"/> is null or whitespace.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task DeleteStateAsync(
            string storeName,
            string key,
            StateOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(storeName));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            var path = $"/v1.0/state/{storeName}/{key}";
            Dictionary<string, string> queryParams = null;

            if (options?.Concurrency != null || options?.Consistency != null)
            {
                queryParams = new Dictionary<string, string>();

                if (options.Concurrency.HasValue)
                {
                    queryParams["concurrency"] = options.Concurrency.Value == ConcurrencyMode.FirstWrite
                        ? "first-write"
                        : "last-write";
                }

                if (options.Consistency.HasValue)
                {
                    queryParams["consistency"] = options.Consistency.Value == ConsistencyMode.Strong
                        ? "strong"
                        : "eventual";
                }
            }

            await _httpClient.DeleteAsync(path, queryParams).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves multiple state items in a single bulk operation.
        /// </summary>
        /// <typeparam name="TValue">The state value type.</typeparam>
        /// <param name="storeName">The name of the state store.</param>
        /// <param name="items">The collection of state items to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="storeName"/> is null or whitespace, or <paramref name="items"/> is null or empty.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task SaveBulkStateAsync<TValue>(
            string storeName,
            IEnumerable<StateItem<TValue>> items)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(storeName));
            }

            if (items == null)
            {
                throw new ArgumentException("Value cannot be null.", nameof(items));
            }

            var itemsList = items.ToList();
            if (itemsList.Count == 0)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(items));
            }

            var requestArray = itemsList.Select(item => new SaveStateRequest
            {
                Key = item.Key,
                Value = item.Value,
                ETag = item.ETag,
            }).ToArray();

            var path = $"/v1.0/state/{storeName}";
            await _httpClient.PostJsonNoContentAsync(path, requestArray).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets multiple state items in a single bulk operation.
        /// </summary>
        /// <typeparam name="TValue">The state value type.</typeparam>
        /// <param name="storeName">The name of the state store.</param>
        /// <param name="keys">The collection of state keys to retrieve.</param>
        /// <param name="parallelism">Optional parallelism level for concurrent retrieval.</param>
        /// <returns>A task representing the asynchronous operation with the collection of state items.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="storeName"/> is null or whitespace, or <paramref name="keys"/> is null or empty.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task<IEnumerable<StateItem<TValue>>> GetBulkStateAsync<TValue>(
            string storeName,
            IEnumerable<string> keys,
            int? parallelism = null)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(storeName));
            }

            if (keys == null)
            {
                throw new ArgumentException("Value cannot be null.", nameof(keys));
            }

            var keysList = keys.ToList();
            if (keysList.Count == 0)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(keys));
            }

            var request = new BulkGetRequest
            {
                Keys = keysList.ToArray(),
                Parallelism = parallelism,
            };

            var path = $"/v1.0/state/{storeName}/bulk";
            var response = await _httpClient.PostJsonArrayAsync<BulkGetRequest, BulkStateItem>(path, request)
                .ConfigureAwait(false);

            return response.Select(item => new StateItem<TValue>
            {
                Key = item.Key,
                Value = item.Data != null
                    ? JsonConvert.DeserializeObject<TValue>(JsonConvert.SerializeObject(item.Data))
                    : default(TValue),
                ETag = item.ETag,
            });
        }

        /// <summary>
        /// Deletes multiple state items in a single bulk operation.
        /// </summary>
        /// <param name="storeName">The name of the state store.</param>
        /// <param name="keys">The collection of state keys to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="storeName"/> is null or whitespace, or <paramref name="keys"/> is null or empty.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default).
        /// </exception>
        public async Task DeleteBulkStateAsync(
            string storeName,
            IEnumerable<string> keys)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(storeName));
            }

            if (keys == null)
            {
                throw new ArgumentException("Value cannot be null.", nameof(keys));
            }

            var keysList = keys.ToList();
            if (keysList.Count == 0)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(keys));
            }

            var requestArray = keysList.Select(key => new SaveStateRequest
            {
                Key = key,
            }).ToArray();

            var path = $"/v1.0/state/{storeName}";
            await _httpClient.PostJsonNoContentAsync(path, requestArray).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes multiple state operations as an atomic transaction.
        /// All operations succeed or fail together.
        /// </summary>
        /// <param name="storeName">The name of the state store (must support transactions).</param>
        /// <param name="operations">The collection of state operations to execute atomically.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="storeName"/> is null or whitespace, or <paramref name="operations"/> is null or empty.
        /// </exception>
        /// <exception cref="DaprException">
        /// Thrown when Dapr is unavailable and Required=true (default), or when the state store does not support transactions.
        /// </exception>
        public async Task ExecuteStateTransactionAsync(
            string storeName,
            IEnumerable<StateTransactionRequest> operations)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(storeName));
            }

            if (operations == null)
            {
                throw new ArgumentException("Value cannot be null.", nameof(operations));
            }

            var operationsList = operations.ToList();
            if (operationsList.Count == 0)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(operations));
            }

            var transactionOperations = operationsList.Select(op => new TransactionOperation
            {
                Operation = op.OperationType == StateOperationType.Upsert ? "upsert" : "delete",
                Request = new SaveStateRequest
                {
                    Key = op.Key,
                    Value = op.Value,
                    ETag = op.ETag,
                },
            }).ToArray();

            var transactionRequest = new StateTransactionRequestInternal
            {
                Operations = transactionOperations,
            };

            var path = $"/v1.0/state/{storeName}/transaction";
            await _httpClient.PostJsonNoContentAsync(path, transactionRequest).ConfigureAwait(false);
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
