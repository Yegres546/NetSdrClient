using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSdrClient.Models;
using NetSdrClient.Services;
using System.Linq;

namespace NetSdrClient.Tests
{
    [TestClass]
    public class SDRClientTests
    {
        [TestMethod]
        public void SDRClient_Constructor_ShouldInitializeDevicesList()
        {
            // Arrange & Act
            var client = new SDRClient();

            // Assert
            Assert.IsNotNull(client.Devices);
            Assert.AreEqual(0, client.Devices.Count);
        }

        [TestMethod]
        public void AddDevice_ShouldAddDeviceToList()
        {
            // Arrange
            var client = new SDRClient();
            var device = new SDRDevice { Id = 1, Name = "Test Device" };

            // Act
            client.AddDevice(device);

            // Assert
            Assert.AreEqual(1, client.Devices.Count);
            Assert.AreEqual(device, client.Devices[0]);
        }

        [TestMethod]
        public void RemoveDevice_ShouldRemoveDeviceFromList()
        {
            // Arrange
            var client = new SDRClient();
            var device = new SDRDevice { Id = 1, Name = "Test Device" };
            client.AddDevice(device);

            // Act
            var result = client.RemoveDevice(1);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, client.Devices.Count);
        }

        [TestMethod]
        public void RemoveDevice_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var client = new SDRClient();

            // Act
            var result = client.RemoveDevice(999);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetDevice_ShouldReturnCorrectDevice()
        {
            // Arrange
            var client = new SDRClient();
            var device1 = new SDRDevice { Id = 1, Name = "Device 1" };
            var device2 = new SDRDevice { Id = 2, Name = "Device 2" };
            client.AddDevice(device1);
            client.AddDevice(device2);

            // Act
            var result = client.GetDevice(2);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Device 2", result.Name);
        }

        [TestMethod]
        public void GetDevice_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var client = new SDRClient();

            // Act
            var result = client.GetDevice(999);

            // Assert
            Assert.IsNull(result);
        }
    }
}
