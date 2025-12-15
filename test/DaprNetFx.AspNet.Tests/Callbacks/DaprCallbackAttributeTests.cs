// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.AspNet.Tests.Callbacks
{
    using System;
    using System.Linq;
    using System.Reflection;
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// Unit tests for DaprCallbackAttribute - marker attribute for Dapr callback actions.
    /// </summary>
    [TestFixture]
    public class DaprCallbackAttributeTests
    {
        [Test]
        public void Attribute_CanBeAppliedToMethod()
        {
            // Arrange
            var method = typeof(TestController).GetMethod(nameof(TestController.CallbackAction));

            // Act
            var attribute = method.GetCustomAttribute<DaprCallbackAttribute>();

            // Assert
            attribute.ShouldNotBeNull();
        }

        [Test]
        public void Attribute_CannotBeAppliedMultipleTimes()
        {
            // Arrange - AttributeUsage with AllowMultiple = false
            var attributeUsage = typeof(DaprCallbackAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();

            // Assert
            attributeUsage.ShouldNotBeNull();
            attributeUsage.AllowMultiple.ShouldBeFalse();
        }

        [Test]
        public void Attribute_CanBeInherited()
        {
            // Arrange - AttributeUsage with Inherited = true
            var attributeUsage = typeof(DaprCallbackAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();

            // Assert
            attributeUsage.ShouldNotBeNull();
            attributeUsage.Inherited.ShouldBeTrue();
        }

        [Test]
        public void Attribute_TargetsMethodsOnly()
        {
            // Arrange
            var attributeUsage = typeof(DaprCallbackAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();

            // Assert
            attributeUsage.ShouldNotBeNull();
            attributeUsage.ValidOn.ShouldBe(AttributeTargets.Method);
        }

        [Test]
        public void Attribute_IsSealed()
        {
            // Arrange
            var type = typeof(DaprCallbackAttribute);

            // Assert
            type.IsSealed.ShouldBeTrue();
        }

        [Test]
        public void Attribute_InheritedMethod_CanAccessAttribute()
        {
            // Arrange - Test inheritance behavior
            var baseMethod = typeof(BaseController).GetMethod(nameof(BaseController.BaseCallbackAction));
            var derivedMethod = typeof(DerivedController).GetMethod(nameof(DerivedController.BaseCallbackAction));

            // Act
            var baseAttribute = baseMethod.GetCustomAttribute<DaprCallbackAttribute>(inherit: false);
            var derivedAttribute = derivedMethod.GetCustomAttribute<DaprCallbackAttribute>(inherit: true);

            // Assert
            baseAttribute.ShouldNotBeNull();
            derivedAttribute.ShouldNotBeNull(); // Inherited = true allows this
        }

        // Test controller for reflection tests
        private class TestController
        {
            [DaprCallback]
            public void CallbackAction()
            {
            }
        }

        private class BaseController
        {
            [DaprCallback]
            public virtual void BaseCallbackAction()
            {
            }
        }

        private class DerivedController : BaseController
        {
            public override void BaseCallbackAction()
            {
            }
        }
    }
}
