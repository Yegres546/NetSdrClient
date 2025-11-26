using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSdrClient.Models;

namespace NetSdrClient.Tests
{
    [TestClass]
    public class SDRDeviceTests
    {
        [TestMethod]
        public void SDRDevice_Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var device = new SDRDevice();

            // Assert
            Assert.IsNotNull(device);
            Assert.AreEqual(0, device.Id);
            Assert.IsNull(device.Name);
            Assert.IsNull(device.Description);
            Assert.IsFalse(device.IsConnected);
        }

        [TestMethod]
        public void SDRDevice_Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var device = new SDRDevice();

            // Act
            device.Id = 1;
            device.Name = "Test Device";
            device.Description = "Test Description";
            device.IsConnected = true;

            // Assert
            Assert.AreEqual(1, device.Id);
            Assert.AreEqual("Test Device", device.Name);
            Assert.AreEqual("Test Description", device.Description);
            Assert.IsTrue(device.IsConnected);
        }

        [TestMethod]
        public void SDRDevice_ToString_ShouldReturnName()
        {
            // Arrange
            var device = new SDRDevice { Name = "Test SDR" };

            // Act
            var result = device.ToString();

            // Assert
            Assert.AreEqual("Test SDR", result);
        }
    }
}
