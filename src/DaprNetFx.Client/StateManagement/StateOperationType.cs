// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    /// <summary>
    /// Defines operation types for state transactions.
    /// </summary>
    public enum StateOperationType
    {
        /// <summary>
        /// Insert or update state (upsert operation).
        /// Creates the key if it doesn't exist, updates if it does.
        /// </summary>
        Upsert,

        /// <summary>
        /// Delete state.
        /// Removes the key from the state store.
        /// </summary>
        Delete,
    }
}
