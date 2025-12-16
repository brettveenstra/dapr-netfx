// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Http.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Internal model for state operation options.
    /// Maps concurrency and consistency enums to Dapr API string values.
    /// </summary>
    internal class StateOperationOptions
    {
        /// <summary>
        /// Gets or sets the concurrency mode.
        /// Valid values: "first-write", "last-write".
        /// </summary>
        [JsonProperty("concurrency", NullValueHandling = NullValueHandling.Ignore)]
        public string Concurrency { get; set; }

        /// <summary>
        /// Gets or sets the consistency mode.
        /// Valid values: "strong", "eventual".
        /// </summary>
        [JsonProperty("consistency", NullValueHandling = NullValueHandling.Ignore)]
        public string Consistency { get; set; }
    }
}
