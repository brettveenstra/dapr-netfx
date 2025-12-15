// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet
{
    using System;
    using System.Web.Http;
    using System.Web.Http.Dependencies;

    /// <summary>
    /// Extension methods for integrating Dapr with ASP.NET Web API.
    /// </summary>
    public static class DaprWebApiExtensions
    {
        /// <summary>
        /// Registers Dapr services with the Web API dependency resolver.
        /// </summary>
        /// <param name="config">The HttpConfiguration to configure.</param>
        /// <param name="options">Optional Dapr client options. If null, uses configuration
        /// from app.config/web.config.</param>
        /// <returns>The HttpConfiguration for chaining.</returns>
        public static HttpConfiguration UseDapr(
            this HttpConfiguration config,
            DaprClientOptions options = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var daprClient = options != null
                ? new DaprClient(options)
                : new DaprClient();

            var currentResolver = config.DependencyResolver;
            config.DependencyResolver = new DaprDependencyResolver(daprClient, currentResolver);

            return config;
        }
    }
}
