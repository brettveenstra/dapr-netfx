// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Http.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Internal request model for bulk get state operations.
    /// </summary>
    internal class BulkGetRequest
    {
        /// <summary>
        /// Gets or sets the array of keys to retrieve.
        /// </summary>
        [JsonProperty("keys")]
        public string[] Keys { get; set; }

        /// <summary>
        /// Gets or sets the parallelism level for concurrent retrieval.
        /// </summary>
        [JsonProperty("parallelism", NullValueHandling = NullValueHandling.Ignore)]
        public int? Parallelism { get; set; }
    }
}
