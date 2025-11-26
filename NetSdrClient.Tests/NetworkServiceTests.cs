using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSdrClient.Services;

namespace NetSdrClient.Tests
{
    [TestClass]
    public class NetworkServiceTests
    {
        [TestMethod]
        public void NetworkService_Constructor_ShouldInitialize()
        {
            // Arrange & Act
            var service = new NetworkService();

            // Assert
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void IsValidIpAddress_WithValidIp_ShouldReturnTrue()
        {
            // Arrange
            var service = new NetworkService();
            var validIp = "192.168.1.1";

            // Act
            var result = service.IsValidIpAddress(validIp);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidIpAddress_WithInvalidIp_ShouldReturnFalse()
        {
            // Arrange
            var service = new NetworkService();
            var invalidIp = "999.999.999.999";

            // Act
            var result = service.IsValidIpAddress(invalidIp);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidPort_WithValidPort_ShouldReturnTrue()
        {
            // Arrange
            var service = new NetworkService();
            var validPort = 8080;

            // Act
            var result = service.IsValidPort(validPort);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidPort_WithInvalidPort_ShouldReturnFalse()
        {
            // Arrange
            var service = new NetworkService();
            var invalidPort = 99999;

            // Act
            var result = service.IsValidPort(invalidPort);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
