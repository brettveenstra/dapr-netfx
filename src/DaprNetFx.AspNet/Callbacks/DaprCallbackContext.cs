// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet
{
    using System;
    using System.Collections.Specialized;

    /// <summary>
    /// Represents metadata about a Dapr service invocation callback.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When Dapr invokes your application via service-to-service invocation,
    /// it includes metadata headers that identify the calling service.
    /// This class provides strongly-typed access to those headers.
    /// </para>
    /// <para>
    /// Access this context via the CallbackContext property on <see cref="DaprApiController"/>.
    /// </para>
    /// </remarks>
    public sealed class DaprCallbackContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaprCallbackContext"/> class.
        /// </summary>
        /// <param name="callerAppId">The app ID of the calling service.</param>
        /// <param name="callerNamespace">The namespace of the calling service.</param>
        /// <param name="calleeAppId">The app ID of the callee service.</param>
        internal DaprCallbackContext(
            string callerAppId,
            string callerNamespace,
            string calleeAppId)
        {
            CallerAppId = callerAppId ?? string.Empty;
            CallerNamespace = callerNamespace ?? string.Empty;
            CalleeAppId = calleeAppId ?? string.Empty;
        }

        /// <summary>
        /// Gets an empty <see cref="DaprCallbackContext"/> with no headers present.
        /// </summary>
        /// <value>
        /// A context instance with all properties set to <see cref="string.Empty"/>.
        /// </value>
        /// <remarks>
        /// Use this when Dapr headers are not available (e.g., unit tests, background threads,
        /// or direct HTTP calls not routed through Dapr).
        /// </remarks>
        public static DaprCallbackContext Empty =>
            new DaprCallbackContext(null, null, null);

        /// <summary>
        /// Gets the app ID of the calling service (dapr-caller-app-id header).
        /// </summary>
        /// <value>
        /// The app ID of the service that invoked this callback, or <see cref="string.Empty"/> if not present.
        /// </value>
        public string CallerAppId { get; }

        /// <summary>
        /// Gets the namespace of the calling service (dapr-caller-namespace header).
        /// </summary>
        /// <value>
        /// The Kubernetes namespace of the calling service, or <see cref="string.Empty"/> if not present.
        /// </value>
        public string CallerNamespace { get; }

        /// <summary>
        /// Gets the app ID of the callee service (dapr-callee-app-id header).
        /// </summary>
        /// <value>
        /// Your application's app ID as registered with Dapr, or <see cref="string.Empty"/> if not present.
        /// </value>
        public string CalleeAppId { get; }

        /// <summary>
        /// Gets a value indicating whether Dapr headers are present in the request.
        /// </summary>
        /// <value>
        /// <c>true</c> if either <see cref="CallerAppId"/> or <see cref="CalleeAppId"/> are present;
        /// otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Use this property to distinguish between Dapr-invoked requests and direct HTTP calls.
        /// Note that <see cref="CallerNamespace"/> alone does not indicate Dapr presence.
        /// </remarks>
        public bool HasDaprHeaders =>
            !string.IsNullOrWhiteSpace(CallerAppId) ||
            !string.IsNullOrWhiteSpace(CalleeAppId);

        /// <summary>
        /// Creates a <see cref="DaprCallbackContext"/> from HTTP headers.
        /// </summary>
        /// <param name="headers">The HTTP headers collection to extract Dapr metadata from.</param>
        /// <returns>A new <see cref="DaprCallbackContext"/> instance with values from the headers.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="headers"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// This method extracts the following headers:
        /// <list type="bullet">
        /// <item><description>dapr-caller-app-id</description></item>
        /// <item><description>dapr-caller-namespace</description></item>
        /// <item><description>dapr-callee-app-id</description></item>
        /// </list>
        /// </remarks>
        public static DaprCallbackContext FromHeaders(NameValueCollection headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            return new DaprCallbackContext(
                headers["dapr-caller-app-id"],
                headers["dapr-caller-namespace"],
                headers["dapr-callee-app-id"]);
        }
    }
}
