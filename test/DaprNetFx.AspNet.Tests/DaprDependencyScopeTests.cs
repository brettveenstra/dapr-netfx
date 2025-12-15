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
    /// Tests for <see cref="DaprDependencyScope"/>.
    /// </summary>
    [TestFixture]
    public class DaprDependencyScopeTests
    {
        private DaprClient _daprClient;
        private IDependencyScope _innerScope;

        [SetUp]
        public void SetUp()
        {
            var options = new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            };

            _daprClient = new DaprClient(options);
            _innerScope = A.Fake<IDependencyScope>();
        }

        [TearDown]
        public void TearDown()
        {
            _daprClient?.Dispose();
            (_innerScope as IDisposable)?.Dispose();
        }

        [Test]
        public void Constructor_WithNullDaprClient_ShouldThrowArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(
                () => new DaprDependencyScope(null, _innerScope));
        }

        [Test]
        public void Constructor_WithNullInnerScope_ShouldSucceed()
        {
            var scope = new DaprDependencyScope(_daprClient, null);

            scope.ShouldNotBeNull();
        }

        [Test]
        public void GetService_WithDaprClientType_ShouldReturnDaprClient()
        {
            var scope = new DaprDependencyScope(_daprClient, _innerScope);

            var result = scope.GetService(typeof(DaprClient));

            result.ShouldBe(_daprClient);
        }

        [Test]
        public void GetService_WithOtherType_ShouldDelegateToInnerScope()
        {
            var scope = new DaprDependencyScope(_daprClient, _innerScope);
            var expectedService = new object();

            A.CallTo(() => _innerScope.GetService(typeof(string)))
                .Returns(expectedService);

            var result = scope.GetService(typeof(string));

            result.ShouldBe(expectedService);
        }

        [Test]
        public void GetService_WithOtherTypeAndNullInnerScope_ShouldReturnNull()
        {
            var scope = new DaprDependencyScope(_daprClient, null);

            var result = scope.GetService(typeof(string));

            result.ShouldBeNull();
        }

        [Test]
        public void GetServices_WithDaprClientType_ShouldReturnDaprClientCollection()
        {
            var scope = new DaprDependencyScope(_daprClient, _innerScope);

            var results = scope.GetServices(typeof(DaprClient)).ToList();

            results.ShouldNotBeEmpty();
            results.Count.ShouldBe(1);
            results[0].ShouldBe(_daprClient);
        }

        [Test]
        public void GetServices_WithOtherType_ShouldDelegateToInnerScope()
        {
            var scope = new DaprDependencyScope(_daprClient, _innerScope);
            var expectedServices = new object[] { new object(), new object() };

            A.CallTo(() => _innerScope.GetServices(typeof(string)))
                .Returns(expectedServices);

            var results = scope.GetServices(typeof(string));

            results.ShouldBe(expectedServices);
        }

        [Test]
        public void GetServices_WithOtherTypeAndNullInnerScope_ShouldReturnEmptyCollection()
        {
            var scope = new DaprDependencyScope(_daprClient, null);

            var results = scope.GetServices(typeof(string)).ToList();

            results.ShouldBeEmpty();
        }

        [Test]
        public void Dispose_ShouldDisposeInnerScope()
        {
            var scope = new DaprDependencyScope(_daprClient, _innerScope);

            scope.Dispose();

            A.CallTo(() => _innerScope.Dispose())
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void Dispose_WithNullInnerScope_ShouldNotThrow()
        {
            var scope = new DaprDependencyScope(_daprClient, null);

            Should.NotThrow(() => scope.Dispose());
        }
    }
}
