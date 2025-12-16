// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet.Tests
{
    using System;
    using System.Linq;
    using System.Web.Http.Dependencies;
    using FakeItEasy;
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// Tests for <see cref="DaprDependencyResolver"/>.
    /// </summary>
    [TestFixture]
    public class DaprDependencyResolverTests
    {
        private DaprClient _daprClient;
        private IDependencyResolver _innerResolver;

        [SetUp]
        public void SetUp()
        {
            var options = new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            };

            _daprClient = new DaprClient(options);
            _innerResolver = A.Fake<IDependencyResolver>();
        }

        [TearDown]
        public void TearDown()
        {
            _daprClient?.Dispose();
            (_innerResolver as IDisposable)?.Dispose();
        }

        [Test]
        public void Constructor_WithNullDaprClient_ShouldThrowArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(
                () => new DaprDependencyResolver(null, _innerResolver));
        }

        [Test]
        public void Constructor_WithNullInnerResolver_ShouldSucceed()
        {
            var resolver = new DaprDependencyResolver(_daprClient, null);

            resolver.ShouldNotBeNull();
        }

        [Test]
        public void GetService_WithDaprClientType_ShouldReturnDaprClient()
        {
            var resolver = new DaprDependencyResolver(_daprClient, _innerResolver);

            var result = resolver.GetService(typeof(DaprClient));

            result.ShouldBe(_daprClient);
        }

        [Test]
        public void GetService_WithOtherType_ShouldDelegateToInnerResolver()
        {
            var resolver = new DaprDependencyResolver(_daprClient, _innerResolver);
            var expectedService = new object();

            A.CallTo(() => _innerResolver.GetService(typeof(string)))
                .Returns(expectedService);

            var result = resolver.GetService(typeof(string));

            result.ShouldBe(expectedService);
        }

        [Test]
        public void GetService_WithOtherTypeAndNullInnerResolver_ShouldReturnNull()
        {
            var resolver = new DaprDependencyResolver(_daprClient, null);

            var result = resolver.GetService(typeof(string));

            result.ShouldBeNull();
        }

        [Test]
        public void GetServices_WithDaprClientType_ShouldReturnDaprClientCollection()
        {
            var resolver = new DaprDependencyResolver(_daprClient, _innerResolver);

            var results = resolver.GetServices(typeof(DaprClient)).ToList();

            results.ShouldNotBeEmpty();
            results.Count.ShouldBe(1);
            results[0].ShouldBe(_daprClient);
        }

        [Test]
        public void GetServices_WithOtherType_ShouldDelegateToInnerResolver()
        {
            var resolver = new DaprDependencyResolver(_daprClient, _innerResolver);
            var expectedServices = new object[] { new object(), new object() };

            A.CallTo(() => _innerResolver.GetServices(typeof(string)))
                .Returns(expectedServices);

            var results = resolver.GetServices(typeof(string));

            results.ShouldBe(expectedServices);
        }

        [Test]
        public void GetServices_WithOtherTypeAndNullInnerResolver_ShouldReturnEmptyCollection()
        {
            var resolver = new DaprDependencyResolver(_daprClient, null);

            var results = resolver.GetServices(typeof(string)).ToList();

            results.ShouldBeEmpty();
        }

        [Test]
        public void BeginScope_ShouldReturnDaprDependencyScope()
        {
            var resolver = new DaprDependencyResolver(_daprClient, _innerResolver);
            var innerScope = A.Fake<IDependencyScope>();

            A.CallTo(() => _innerResolver.BeginScope())
                .Returns(innerScope);

            var scope = resolver.BeginScope();

            scope.ShouldNotBeNull();
            scope.ShouldBeOfType<DaprDependencyScope>();
        }

        [Test]
        public void BeginScope_WithNullInnerResolver_ShouldReturnDaprDependencyScope()
        {
            var resolver = new DaprDependencyResolver(_daprClient, null);

            var scope = resolver.BeginScope();

            scope.ShouldNotBeNull();
            scope.ShouldBeOfType<DaprDependencyScope>();
        }

        [Test]
        public void Dispose_ShouldDisposeInnerResolver()
        {
            var resolver = new DaprDependencyResolver(_daprClient, _innerResolver);

            resolver.Dispose();

            A.CallTo(() => _innerResolver.Dispose())
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void Dispose_WithNullInnerResolver_ShouldNotThrow()
        {
            var resolver = new DaprDependencyResolver(_daprClient, null);

            Should.NotThrow(() => resolver.Dispose());
        }

        [Test]
        public void Dispose_ShouldPreventSubsequentUse()
        {
            // Arrange - Create real instances
            var options = new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            };
            var client = new DaprClient(options);
            var resolver = new DaprDependencyResolver(client, null);

            // Act - Dispose the resolver
            resolver.Dispose();

            // Assert - Subsequent use should throw ObjectDisposedException
            Should.Throw<ObjectDisposedException>(() => resolver.GetService(typeof(DaprClient)));
        }
    }
}
