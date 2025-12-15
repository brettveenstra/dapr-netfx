// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet.Tests
{
    using System;
    using System.Web.Http;
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// Tests for <see cref="DaprWebApiExtensions"/>.
    /// </summary>
    [TestFixture]
    public class DaprWebApiExtensionsTests
    {
        [Test]
        public void UseDapr_WithNullConfig_ShouldThrowArgumentNullException()
        {
            HttpConfiguration config = null;

            Should.Throw<ArgumentNullException>(() => config.UseDapr());
        }

        [Test]
        public void UseDapr_WithValidConfig_ShouldReturnConfig()
        {
            var config = new HttpConfiguration();

            var result = config.UseDapr(new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            });

            result.ShouldBe(config);
        }

        [Test]
        public void UseDapr_WithValidConfig_ShouldReplaceDependencyResolver()
        {
            var config = new HttpConfiguration();
            var originalResolver = config.DependencyResolver;

            config.UseDapr(new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            });

            config.DependencyResolver.ShouldNotBe(originalResolver);
            config.DependencyResolver.ShouldBeOfType<DaprDependencyResolver>();
        }

        [Test]
        public void UseDapr_WithOptions_ShouldRegisterDaprClient()
        {
            var config = new HttpConfiguration();

            config.UseDapr(new DaprClientOptions
            {
                HttpEndpoint = "http://localhost:3500",
                Required = false
            });

            var daprClient = config.DependencyResolver.GetService(typeof(DaprClient));
            daprClient.ShouldNotBeNull();
            daprClient.ShouldBeOfType<DaprClient>();
        }

        [Test]
        public void UseDapr_WithNullOptions_ShouldUseDefaultConfiguration()
        {
            var config = new HttpConfiguration();

            config.UseDapr(options: null);

            var daprClient = config.DependencyResolver.GetService(typeof(DaprClient));
            daprClient.ShouldNotBeNull();
            daprClient.ShouldBeOfType<DaprClient>();
        }

        [Test]
        public void UseDapr_ShouldAllowMethodChaining()
        {
            var config = new HttpConfiguration();

            var result = config
                .UseDapr(new DaprClientOptions
                {
                    HttpEndpoint = "http://localhost:3500",
                    Required = false
                });

            result.ShouldBe(config);
        }
    }
}
