// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Tests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// Tests for <see cref="DaprException"/>.
    /// </summary>
    [TestFixture]
    public class DaprExceptionTests
    {
        [Test]
        public void Constructor_Default_ShouldSucceed()
        {
            var exception = new DaprException();

            exception.ShouldNotBeNull();
            exception.Message.ShouldNotBeNullOrWhiteSpace();
        }

        [Test]
        public void Constructor_WithMessage_ShouldSetMessage()
        {
            var exception = new DaprException("Test error message");

            exception.Message.ShouldBe("Test error message");
        }

        [Test]
        public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
        {
            var innerException = new InvalidOperationException("Inner error");
            var exception = new DaprException("Test error", innerException);

            exception.Message.ShouldBe("Test error");
            exception.InnerException.ShouldBe(innerException);
        }

        [Test]
        public void Serialization_ShouldPreserveExceptionDetails()
        {
            var original = new DaprException("Serialization test");

            // Serialize
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, original);
                stream.Position = 0;

                // Deserialize
                var deserialized = (DaprException)formatter.Deserialize(stream);

                deserialized.Message.ShouldBe(original.Message);
            }
        }
    }
}
