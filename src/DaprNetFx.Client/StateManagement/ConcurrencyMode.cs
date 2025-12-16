// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    /// <summary>
    /// Defines concurrency modes for state operations.
    /// </summary>
    public enum ConcurrencyMode
    {
        /// <summary>
        /// First write wins (optimistic locking with ETag validation).
        /// Updates fail if ETag doesn't match current value.
        /// </summary>
        FirstWrite,

        /// <summary>
        /// Last write wins (default behavior, no ETag validation).
        /// Latest write always succeeds regardless of concurrent modifications.
        /// </summary>
        LastWrite,
    }
}
