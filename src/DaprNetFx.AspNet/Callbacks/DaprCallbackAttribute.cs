// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet
{
    using System;

    /// <summary>
    /// Marks a controller action as a Dapr service invocation callback endpoint.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Apply this attribute to action methods that receive inbound service invocations from Dapr.
    /// Actions marked with this attribute can access callback metadata via the
    /// CallbackContext property on <see cref="DaprApiController"/>.
    /// </para>
    /// <para>
    /// This attribute is a marker for documentation purposes. No runtime validation is performed
    /// to maintain consistency with POC1 patterns (trust developers, minimal overhead).
    /// </para>
    /// <example>
    /// <code>
    /// public class OrderController : DaprApiController
    /// {
    ///     [DaprCallback]
    ///     [HttpPost]
    ///     public IHttpActionResult ProcessOrder(Order order)
    ///     {
    ///         var callerAppId = this.CallbackContext.CallerAppId;
    ///         // Process order...
    ///         return Ok();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DaprCallbackAttribute : Attribute
    {
        // Simple marker attribute - no properties or runtime validation for POC2.
        // Future enhancements could add:
        // - RequireAuthentication property
        // - AllowedCallers string[] property
        // - MethodName property for custom routing
    }
}
