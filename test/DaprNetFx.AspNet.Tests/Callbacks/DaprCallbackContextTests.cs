using System.Collections.Specialized;
using NUnit.Framework;
using Shouldly;

namespace DaprNetFx.AspNet.Tests.Callbacks
{
    /// <summary>
    /// Unit tests for DaprCallbackContext - metadata container for Dapr callback invocations.
    /// </summary>
    [TestFixture]
    public class DaprCallbackContextTests
    {
        [Test]
        public void Constructor_WithAllNulls_ShouldReturnEmptyStrings()
        {
            // Arrange & Act
            var context = new DaprCallbackContext(null, null, null);

            // Assert
            context.CallerAppId.ShouldBe(string.Empty);
            context.CallerNamespace.ShouldBe(string.Empty);
            context.CalleeAppId.ShouldBe(string.Empty);
        }

        [Test]
        public void Constructor_WithValidValues_ShouldSetProperties()
        {
            // Arrange & Act
            var context = new DaprCallbackContext(
                callerAppId: "inventory-service",
                callerNamespace: "production",
                calleeAppId: "order-service"
            );

            // Assert
            context.CallerAppId.ShouldBe("inventory-service");
            context.CallerNamespace.ShouldBe("production");
            context.CalleeAppId.ShouldBe("order-service");
        }

        [Test]
        public void Constructor_WithEmptyStrings_ShouldKeepEmptyStrings()
        {
            // Arrange & Act
            var context = new DaprCallbackContext(
                callerAppId: string.Empty,
                callerNamespace: string.Empty,
                calleeAppId: string.Empty
            );

            // Assert
            context.CallerAppId.ShouldBe(string.Empty);
            context.CallerNamespace.ShouldBe(string.Empty);
            context.CalleeAppId.ShouldBe(string.Empty);
        }

        [Test]
        public void HasDaprHeaders_WhenAllHeadersNull_ShouldReturnFalse()
        {
            // Arrange
            var context = new DaprCallbackContext(null, null, null);

            // Act & Assert
            context.HasDaprHeaders.ShouldBeFalse();
        }

        [Test]
        public void HasDaprHeaders_WhenAllHeadersEmpty_ShouldReturnFalse()
        {
            // Arrange
            var context = new DaprCallbackContext(
                string.Empty,
                string.Empty,
                string.Empty
            );

            // Act & Assert
            context.HasDaprHeaders.ShouldBeFalse();
        }

        [Test]
        public void HasDaprHeaders_WhenCallerAppIdPresent_ShouldReturnTrue()
        {
            // Arrange
            var context = new DaprCallbackContext(
                callerAppId: "some-service",
                callerNamespace: null,
                calleeAppId: null
            );

            // Act & Assert
            context.HasDaprHeaders.ShouldBeTrue();
        }

        [Test]
        public void HasDaprHeaders_WhenCalleeAppIdPresent_ShouldReturnTrue()
        {
            // Arrange
            var context = new DaprCallbackContext(
                callerAppId: null,
                callerNamespace: null,
                calleeAppId: "order-service"
            );

            // Act & Assert
            context.HasDaprHeaders.ShouldBeTrue();
        }

        [Test]
        public void HasDaprHeaders_WhenOnlyNamespacePresent_ShouldReturnFalse()
        {
            // Arrange - Namespace alone doesn't indicate Dapr presence
            var context = new DaprCallbackContext(
                callerAppId: null,
                callerNamespace: "production",
                calleeAppId: null
            );

            // Act & Assert
            context.HasDaprHeaders.ShouldBeFalse();
        }

        [Test]
        public void FromHeaders_WithNullHeaders_ShouldThrowArgumentNullException()
        {
            // Arrange
            NameValueCollection headers = null;

            // Act & Assert
            Should.Throw<System.ArgumentNullException>(() =>
            {
                DaprCallbackContext.FromHeaders(headers);
            }).ParamName.ShouldBe("headers");
        }

        [Test]
        public void FromHeaders_WithDaprHeaders_ShouldExtractValues()
        {
            // Arrange
            var headers = new NameValueCollection
            {
                { "dapr-caller-app-id", "inventory-service" },
                { "dapr-caller-namespace", "production" },
                { "dapr-callee-app-id", "order-service" }
            };

            // Act
            var context = DaprCallbackContext.FromHeaders(headers);

            // Assert
            context.CallerAppId.ShouldBe("inventory-service");
            context.CallerNamespace.ShouldBe("production");
            context.CalleeAppId.ShouldBe("order-service");
        }

        [Test]
        public void FromHeaders_WithMissingHeaders_ShouldReturnEmptyStrings()
        {
            // Arrange
            var headers = new NameValueCollection
            {
                { "Content-Type", "application/json" },
                { "User-Agent", "Dapr/1.0" }
            };

            // Act
            var context = DaprCallbackContext.FromHeaders(headers);

            // Assert
            context.CallerAppId.ShouldBe(string.Empty);
            context.CallerNamespace.ShouldBe(string.Empty);
            context.CalleeAppId.ShouldBe(string.Empty);
            context.HasDaprHeaders.ShouldBeFalse();
        }

        [Test]
        public void FromHeaders_WithPartialHeaders_ShouldExtractAvailableValues()
        {
            // Arrange - Only caller-app-id present
            var headers = new NameValueCollection
            {
                { "dapr-caller-app-id", "inventory-service" }
            };

            // Act
            var context = DaprCallbackContext.FromHeaders(headers);

            // Assert
            context.CallerAppId.ShouldBe("inventory-service");
            context.CallerNamespace.ShouldBe(string.Empty);
            context.CalleeAppId.ShouldBe(string.Empty);
            context.HasDaprHeaders.ShouldBeTrue(); // CallerAppId present
        }

        [Test]
        public void Empty_ShouldReturnContextWithNoHeaders()
        {
            // Act
            var context = DaprCallbackContext.Empty;

            // Assert
            context.CallerAppId.ShouldBe(string.Empty);
            context.CallerNamespace.ShouldBe(string.Empty);
            context.CalleeAppId.ShouldBe(string.Empty);
            context.HasDaprHeaders.ShouldBeFalse();
        }
    }
}
