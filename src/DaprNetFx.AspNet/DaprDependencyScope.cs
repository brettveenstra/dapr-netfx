// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Dependencies;

    /// <summary>
    /// Dependency scope that provides DaprClient instances within a request scope.
    /// </summary>
    internal class DaprDependencyScope : IDependencyScope
    {
        private readonly DaprClient _daprClient;
        private readonly IDependencyScope _innerScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprDependencyScope"/> class.
        /// </summary>
        /// <param name="daprClient">The DaprClient instance to provide.</param>
        /// <param name="innerScope">The existing scope to wrap.</param>
        public DaprDependencyScope(DaprClient daprClient, IDependencyScope innerScope)
        {
            _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            _innerScope = innerScope;
        }

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(DaprClient))
            {
                return _daprClient;
            }

            return _innerScope?.GetService(serviceType);
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == typeof(DaprClient))
            {
                return new[] { _daprClient };
            }

            return _innerScope?.GetServices(serviceType)
                ?? new object[0];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _innerScope?.Dispose();
        }
    }
}
