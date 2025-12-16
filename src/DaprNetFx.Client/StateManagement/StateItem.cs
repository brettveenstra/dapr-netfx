// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    /// <summary>
    /// Represents a state item with key, value, and optional ETag for bulk operations.
    /// </summary>
    /// <typeparam name="TValue">The type of the state value.</typeparam>
    public class StateItem<TValue>
    {
        /// <summary>
        /// Gets or sets the state key.
        /// </summary>
        /// <value>
        /// The unique identifier for this state item within the state store.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the state value.
        /// </summary>
        /// <value>
        /// The state data to store. Will be serialized to JSON.
        /// </value>
        public TValue Value { get; set; }

        /// <summary>
        /// Gets or sets the ETag for optimistic concurrency control.
        /// </summary>
        /// <value>
        /// The ETag value from a previous read operation. Optional for save operations.
        /// </value>
        public string ETag { get; set; }
    }
}
