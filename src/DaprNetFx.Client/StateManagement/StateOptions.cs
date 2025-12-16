// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    /// <summary>
    /// Options for state operations including concurrency control, consistency, and TTL.
    /// </summary>
    public class StateOptions
    {
        /// <summary>
        /// Gets or sets the concurrency mode for optimistic locking.
        /// </summary>
        /// <value>
        /// The concurrency mode. Defaults to null (uses last-write-wins).
        /// </value>
        public ConcurrencyMode? Concurrency { get; set; }

        /// <summary>
        /// Gets or sets the consistency mode for replication.
        /// </summary>
        /// <value>
        /// The consistency mode. Defaults to null (uses eventual consistency).
        /// </value>
        public ConsistencyMode? Consistency { get; set; }

        /// <summary>
        /// Gets or sets the ETag for optimistic concurrency control.
        /// </summary>
        /// <value>
        /// The ETag value from a previous read operation. Used with FirstWrite concurrency mode.
        /// </value>
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live in seconds for state expiration.
        /// </summary>
        /// <value>
        /// The TTL in seconds. State store must support TTL metadata. Defaults to null (no expiration).
        /// </value>
        public int? TtlInSeconds { get; set; }
    }
}
