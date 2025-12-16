// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    /// <summary>
    /// Represents a state transaction operation (upsert or delete) for atomic execution.
    /// </summary>
    public class StateTransactionRequest
    {
        /// <summary>
        /// Gets or sets the operation type (upsert or delete).
        /// </summary>
        /// <value>
        /// The type of operation to perform in the transaction.
        /// </value>
        public StateOperationType OperationType { get; set; }

        /// <summary>
        /// Gets or sets the state key.
        /// </summary>
        /// <value>
        /// The unique identifier for this state item within the state store.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the state value (required for upsert operations, ignored for delete).
        /// </summary>
        /// <value>
        /// The state data to store. Will be serialized to JSON. Required for Upsert, optional for Delete.
        /// </value>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the ETag for optimistic concurrency control.
        /// </summary>
        /// <value>
        /// The ETag value from a previous read operation. Optional.
        /// </value>
        public string ETag { get; set; }
    }
}
