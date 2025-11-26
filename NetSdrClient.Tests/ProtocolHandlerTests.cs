using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSdrClient.Services;

namespace NetSdrClient.Tests
{
    [TestClass]
    public class ProtocolHandlerTests
    {
        [TestMethod]
        public void ProtocolHandler_Constructor_ShouldInitialize()
        {
            // Arrange & Act
            var handler = new ProtocolHandler();

            // Assert
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void CreateCommand_ShouldReturnValidCommand()
        {
            // Arrange
            var handler = new ProtocolHandler();
            var expectedCommand = "CONNECT";

            // Act
            var result = handler.CreateCommand(expectedCommand);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(expectedCommand));
        }

        [TestMethod]
        public void ParseResponse_WithValidData_ShouldReturnParsedResponse()
        {
            // Arrange
            var handler = new ProtocolHandler();
            var testData = "OK:CONNECTED";

            // Act
            var result = handler.ParseResponse(testData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("CONNECTED"));
        }

        [TestMethod]
        public void ParseResponse_WithNullData_ShouldReturnErrorMessage()
        {
            // Arrange
            var handler = new ProtocolHandler();

            // Act
            var result = handler.ParseResponse(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("ERROR"));
        }
    }
}
