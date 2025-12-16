// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Http.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Internal request model for saving state to Dapr.
    /// Maps to Dapr's state save API format.
    /// </summary>
    internal class SaveStateRequest
    {
        /// <summary>
        /// Gets or sets the state key.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the state value.
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Include)]
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the ETag for optimistic concurrency control.
        /// </summary>
        [JsonProperty("etag", NullValueHandling = NullValueHandling.Ignore)]
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets the state operation options (concurrency, consistency).
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public StateOperationOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the metadata for this state operation.
        /// </summary>
        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public object Metadata { get; set; }
    }
}
