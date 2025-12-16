// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Http.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Internal model for a single operation in a state transaction.
    /// </summary>
    internal class TransactionOperation
    {
        /// <summary>
        /// Gets or sets the operation type.
        /// Valid values: "upsert", "delete".
        /// </summary>
        [JsonProperty("operation")]
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the request details for this operation.
        /// Contains key, value, etag, and options.
        /// </summary>
        [JsonProperty("request")]
        public SaveStateRequest Request { get; set; }
    }
}
