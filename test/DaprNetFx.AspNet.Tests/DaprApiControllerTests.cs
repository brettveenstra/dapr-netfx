// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet.Tests
{
    using System;
    using System.Web.Http;
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// Tests for <see cref="DaprApiController"/>.
    /// </summary>
    [TestFixture]
    public class DaprApiControllerTests
    {
        [Test]
        public void Dapr_WhenDaprClientRegistered_ShouldReturnDaprClient()
        {
            var config = new HttpConfiguration();
            config.UseDapr(new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            });

            using (var controller = new TestApiController())
            {
                controller.Configuration = config;

                var daprClient = controller.GetDapr();

                daprClient.ShouldNotBeNull();
                daprClient.ShouldBeOfType<DaprClient>();
            }
        }

        [Test]
        public void Dapr_WhenDaprClientNotRegistered_ShouldThrowInvalidOperationException()
        {
            var config = new HttpConfiguration();

            using (var controller = new TestApiController())
            {
                controller.Configuration = config;

                var exception = Should.Throw<InvalidOperationException>(
                    () => controller.GetDapr());

                exception.Message.ShouldContain("DaprClient is not registered");
                exception.Message.ShouldContain("UseDapr");
            }
        }

        [Test]
        public void Dapr_ShouldCacheDaprClientInstance()
        {
            var config = new HttpConfiguration();
            config.UseDapr(new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            });

            using (var controller = new TestApiController())
            {
                controller.Configuration = config;

                var daprClient1 = controller.GetDapr();
                var daprClient2 = controller.GetDapr();

                daprClient1.ShouldBe(daprClient2);
            }
        }

        [Test]
        public void Dispose_ShouldNotDisposeDaprClient()
        {
            var config = new HttpConfiguration();
            config.UseDapr(new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            });

            var controller = new TestApiController();
            controller.Configuration = config;

            var daprClient = controller.GetDapr();

            // Dispose controller
            controller.Dispose();

            // DaprClient should still be accessible from resolver
            // (not disposed by controller)
            var daprClientFromResolver =
                (DaprClient)config.DependencyResolver.GetService(typeof(DaprClient));

            daprClientFromResolver.ShouldNotBeNull();
        }

        [Test]
        public void Dispose_CalledMultipleTimes_ShouldNotThrow()
        {
            var config = new HttpConfiguration();
            config.UseDapr(new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            });

            var controller = new TestApiController();
            controller.Configuration = config;

            Should.NotThrow(() =>
            {
                controller.Dispose();
                controller.Dispose();
                controller.Dispose();
            });
        }

        /// <summary>
        /// Test controller that exposes the protected Dapr property for testing.
        /// </summary>
        private class TestApiController : DaprApiController
        {
            public DaprClient GetDapr() => this.Dapr;
        }
    }
}
