// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Http.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Internal request model for state transactions.
    /// Wraps an array of operations to be executed atomically.
    /// </summary>
    internal class StateTransactionRequestInternal
    {
        /// <summary>
        /// Gets or sets the array of operations to execute in this transaction.
        /// </summary>
        [JsonProperty("operations")]
        public TransactionOperation[] Operations { get; set; }

        /// <summary>
        /// Gets or sets optional metadata for the transaction.
        /// </summary>
        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public object Metadata { get; set; }
    }
}
