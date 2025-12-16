// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx
{
    using System;

    /// <summary>
    /// Configuration options for the Dapr client.
    /// </summary>
    public class DaprClientOptions
    {
        private const string DefaultHttpEndpoint = "http://localhost:3500";
        private const int DefaultHttpTimeoutSeconds = 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprClientOptions"/> class with default values.
        /// </summary>
        public DaprClientOptions()
        {
            this.HttpEndpoint = DefaultHttpEndpoint;
            this.HttpTimeout = TimeSpan.FromSeconds(DefaultHttpTimeoutSeconds);
            this.Required = true;
        }

        /// <summary>
        /// Gets or sets the Dapr HTTP endpoint.
        /// </summary>
        /// <value>The HTTP endpoint URL. Defaults to http://localhost:3500 for sidecar pattern.</value>
        public string HttpEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the Dapr API token for authentication.
        /// </summary>
        /// <value>The API token, or null if authentication is not required (localhost sidecar).</value>
        public string ApiToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Dapr is required.
        /// </summary>
        /// <value>
        /// True to throw <see cref="DaprException"/> if Dapr is unavailable (fail-fast, default).
        /// False to allow graceful degradation.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request timeout.
        /// </summary>
        /// <value>
        /// The timeout duration. Defaults to 5 seconds (optimized for sidecar pattern with ~2ms latency).
        /// Override to 10-30 seconds for remote Dapr deployments via app.config or environment variable.
        /// </value>
        public TimeSpan HttpTimeout { get; set; }
    }
}
