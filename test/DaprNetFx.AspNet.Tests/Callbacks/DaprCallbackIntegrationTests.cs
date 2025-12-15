// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet.Tests.Callbacks
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// Integration tests for Dapr callback functionality with ASP.NET Web API pipeline.
    /// </summary>
    /// <remarks>
    /// Note: HttpServer self-host does NOT populate HttpContext.Current, so CallbackContext
    /// will return Empty in these tests. Full integration testing requires IIS Express or real ASP.NET pipeline.
    /// </remarks>
    [TestFixture]
    public class DaprCallbackIntegrationTests
    {
        [Test]
        public async Task CallbackAction_WithHttpServer_ShouldNotThrow()
        {
            // Arrange: In-memory Web API host
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "default",
                routeTemplate: "api/{controller}/{action}");

            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                // Add Dapr headers (won't be accessible via HttpContext.Current in self-host)
                client.DefaultRequestHeaders.Add("dapr-caller-app-id", "test-caller");
                client.DefaultRequestHeaders.Add("dapr-caller-namespace", "default");
                client.DefaultRequestHeaders.Add("dapr-callee-app-id", "test-callee");

                // Act: POST to callback endpoint - should not throw even without HttpContext
                var response = await client.PostAsJsonAsync(
                    "http://localhost/api/testcallback/process",
                    new { orderId = 123 });

                // Assert: Request should complete successfully
                response.IsSuccessStatusCode.ShouldBeTrue();
            }
        }

        [Test]
        public async Task CallbackAction_WithoutHeaders_ShouldStillSucceed()
        {
            // Arrange: No Dapr headers (direct HTTP call simulation)
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "default",
                routeTemplate: "api/{controller}/{action}");

            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                // Act: Direct call without Dapr headers
                var response = await client.PostAsJsonAsync(
                    "http://localhost/api/testcallback/process",
                    new { orderId = 456 });

                // Assert: Should handle gracefully (Empty context)
                response.IsSuccessStatusCode.ShouldBeTrue();
            }
        }

        [Test]
        public void CallbackContext_WhenHttpContextUnavailable_ShouldReturnEmpty()
        {
            // Arrange: Controller instantiated directly (no HTTP pipeline)
            var controller = new TestCallbackController();

            // Act: Access CallbackContext without HTTP request
            var context = controller.GetCallbackContextForTest();

            // Assert: Should return Empty context (not throw)
            context.ShouldNotBeNull();
            context.HasDaprHeaders.ShouldBeFalse();
            context.CallerAppId.ShouldBe(string.Empty);
            context.CallerNamespace.ShouldBe(string.Empty);
            context.CalleeAppId.ShouldBe(string.Empty);
        }

        [Test]
        public void CallbackContext_MultipleCalls_ShouldCacheContext()
        {
            // Arrange
            var controller = new TestCallbackController();

            // Act: Access CallbackContext multiple times
            var context1 = controller.GetCallbackContextForTest();
            var context2 = controller.GetCallbackContextForTest();

            // Assert: Should return same cached instance
            ReferenceEquals(context1, context2).ShouldBeTrue();
        }

        [Test]
        public void CallbackContext_AfterDispose_ShouldClearCache()
        {
            // Arrange
            var controller = new TestCallbackController();
            var context1 = controller.GetCallbackContextForTest();

            // Act: Dispose and access again
            controller.Dispose();
            var context2 = controller.GetCallbackContextForTest();

            // Assert: Should create new instance after dispose
            ReferenceEquals(context1, context2).ShouldBeFalse();
        }

        [Test]
        public void DaprCallbackAttribute_CanBeAppliedToActions()
        {
            // Arrange & Act: Reflection test to verify attribute is applied
            var method = typeof(TestCallbackController).GetMethod(nameof(TestCallbackController.Process));
            var attribute = method.GetCustomAttributes(typeof(DaprCallbackAttribute), false);

            // Assert
            attribute.ShouldNotBeEmpty();
            attribute.Length.ShouldBe(1);
        }

        // Test controller for integration tests
        public class TestCallbackController : DaprApiController
        {
            [DaprCallback]
            [HttpPost]
            public IHttpActionResult Process(object request)
            {
                // Access CallbackContext (will be Empty in self-hosted tests)
                var callerAppId = this.CallbackContext.CallerAppId;

                return Ok(new { caller = callerAppId, status = "processed" });
            }

            // Expose CallbackContext for testing
            public DaprCallbackContext GetCallbackContextForTest()
            {
                return this.CallbackContext;
            }
        }
    }
}
