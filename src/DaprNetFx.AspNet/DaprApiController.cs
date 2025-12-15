// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet
{
    using System;
    using System.Web.Http;

    /// <summary>
    /// Base controller class that provides access to DaprClient.
    /// Controllers can inherit from this class to easily access Dapr functionality.
    /// </summary>
    public abstract class DaprApiController : ApiController
    {
        private DaprClient _daprClient;

        /// <summary>
        /// Gets the DaprClient instance for this controller.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if DaprClient is not registered in the dependency resolver.
        /// Call UseDapr() on HttpConfiguration during application startup.
        /// </exception>
        protected DaprClient Dapr
        {
            get
            {
                if (_daprClient == null)
                {
                    _daprClient = (DaprClient)this.Configuration.DependencyResolver
                        .GetService(typeof(DaprClient));

                    if (_daprClient == null)
                    {
                        throw new InvalidOperationException(
                            "DaprClient is not registered. " +
                            "Call config.UseDapr() in Application_Start or WebApiConfig.Register.");
                    }
                }

                return _daprClient;
            }
        }

        /// <summary>
        /// Disposes the controller and ensures DaprClient is not disposed
        /// (it's managed by the dependency resolver).
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Don't dispose _daprClient - it's managed by DependencyResolver
            _daprClient = null;
            base.Dispose(disposing);
        }
    }
}
