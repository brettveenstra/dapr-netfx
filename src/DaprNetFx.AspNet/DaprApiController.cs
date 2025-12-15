// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet
{
    using System;
    using System.Web;
    using System.Web.Http;

    /// <summary>
    /// Base controller class that provides access to DaprClient.
    /// Controllers can inherit from this class to easily access Dapr functionality.
    /// </summary>
    public abstract class DaprApiController : ApiController
    {
        private DaprClient _daprClient;
        private DaprCallbackContext _callbackContext;

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
        /// Gets the Dapr callback context for the current request.
        /// Contains metadata sent by Dapr when invoking this service.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property provides metadata about the calling service (app ID, namespace)
        /// when Dapr invokes your application via service-to-service invocation.
        /// </para>
        /// <para>
        /// Returns a context with empty values if Dapr headers are not present
        /// (direct HTTP call) or if HttpContext is unavailable (unit tests).
        /// </para>
        /// </remarks>
        protected DaprCallbackContext CallbackContext
        {
            get
            {
                if (_callbackContext == null)
                {
                    _callbackContext = ExtractCallbackContext();
                }

                return _callbackContext;
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
            _callbackContext = null;
            base.Dispose(disposing);
        }

        private static DaprCallbackContext ExtractCallbackContext()
        {
            // ASP.NET Web API 5.3.0: Custom headers are in HttpContext.Current.Request.Headers
            // (NOT in ApiController.Request.Headers which is HttpRequestMessage.Headers)
            var context = System.Web.HttpContext.Current;
            if (context?.Request?.Headers == null)
            {
                // HttpContext not available (unit tests, background threads)
                return DaprCallbackContext.Empty;
            }

            var headers = context.Request.Headers;
            return new DaprCallbackContext(
                headers["dapr-caller-app-id"],
                headers["dapr-caller-namespace"],
                headers["dapr-callee-app-id"]);
        }
    }
}
