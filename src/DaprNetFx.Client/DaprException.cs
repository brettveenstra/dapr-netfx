// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown when Dapr operations fail.
    /// </summary>
    [Serializable]
    public class DaprException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprException"/> class.
        /// </summary>
        public DaprException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprException"/> class with a message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public DaprException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprException"/> class with a message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DaprException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprException"/> class with serialized data.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected DaprException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
