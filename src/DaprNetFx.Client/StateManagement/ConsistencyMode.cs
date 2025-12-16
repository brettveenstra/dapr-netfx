// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    /// <summary>
    /// Defines consistency modes for state operations.
    /// </summary>
    public enum ConsistencyMode
    {
        /// <summary>
        /// Eventual consistency (default). Dapr returns as soon as write is accepted by the underlying data store.
        /// Replicas may not be immediately consistent.
        /// </summary>
        Eventual,

        /// <summary>
        /// Strong consistency. Dapr waits for all replicas (or designated quorums) to acknowledge before returning.
        /// Guarantees all replicas are consistent before operation completes.
        /// </summary>
        Strong,
    }
}
