// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Dependencies;

    /// <summary>
    /// Dependency resolver that provides DaprClient instances to Web API controllers.
    /// Wraps an existing IDependencyResolver and adds Dapr-specific services.
    /// </summary>
    internal class DaprDependencyResolver : IDependencyResolver
    {
        private readonly DaprClient _daprClient;
        private readonly IDependencyResolver _innerResolver;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprDependencyResolver"/> class.
        /// </summary>
        /// <param name="daprClient">The DaprClient instance to provide.</param>
        /// <param name="innerResolver">The existing resolver to wrap.</param>
        public DaprDependencyResolver(DaprClient daprClient, IDependencyResolver innerResolver)
        {
            _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            _innerResolver = innerResolver;
        }

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DaprDependencyResolver));
            }

            if (serviceType == typeof(DaprClient))
            {
                return _daprClient;
            }

            return _innerResolver?.GetService(serviceType);
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DaprDependencyResolver));
            }

            if (serviceType == typeof(DaprClient))
            {
                return new[] { _daprClient };
            }

            return _innerResolver?.GetServices(serviceType)
                ?? new object[0];
        }

        /// <inheritdoc/>
        public IDependencyScope BeginScope()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DaprDependencyResolver));
            }

            var innerScope = _innerResolver?.BeginScope();
            return new DaprDependencyScope(_daprClient, innerScope);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _daprClient?.Dispose();
                _innerResolver?.Dispose();
                _disposed = true;
            }
        }
    }
}
