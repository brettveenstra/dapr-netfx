// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Tests
{
    using System;
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// Tests for <see cref="DaprClientOptions"/>.
    /// </summary>
    [TestFixture]
    public class DaprClientOptionsTests
    {
        [Test]
        public void Constructor_ShouldSetDefaultValues()
        {
            var options = new DaprClientOptions();

            options.HttpEndpoint.ShouldBe("http://localhost:3500");
            options.HttpTimeout.ShouldBe(TimeSpan.FromSeconds(5));
            options.Required.ShouldBeTrue();
            options.ApiToken.ShouldBeNull();
        }

        [Test]
        public void HttpEndpoint_ShouldBeSettable()
        {
            var options = new DaprClientOptions
            {
                HttpEndpoint = "https://dapr-cluster.example.com"
            };

            options.HttpEndpoint.ShouldBe("https://dapr-cluster.example.com");
        }

        [Test]
        public void ApiToken_ShouldBeSettable()
        {
            var options = new DaprClientOptions
            {
                ApiToken = "test-token-12345"
            };

            options.ApiToken.ShouldBe("test-token-12345");
        }

        [Test]
        public void Required_ShouldBeSettableToFalse()
        {
            var options = new DaprClientOptions
            {
                Required = false
            };

            options.Required.ShouldBeFalse();
        }

        [Test]
        public void HttpTimeout_ShouldBeSettable()
        {
            var options = new DaprClientOptions
            {
                HttpTimeout = TimeSpan.FromMinutes(5)
            };

            options.HttpTimeout.ShouldBe(TimeSpan.FromMinutes(5));
        }
    }
}
