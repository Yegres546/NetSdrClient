using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSdrClientApp;
using NetSdrClientApp.Models;

namespace NetSdrClientAppTests
{
    [TestClass]
    public class NetSdrClientTests
    {
        [TestMethod]
        public void NetSdrClient_AddDevice_ShouldAddDeviceToList()
        {
            // Arrange
            var client = new NetSdrClient();
            var device = new SDRDevice { Id = 1, Name = "Test Device" };

            // Act
            client.AddDevice(device);

            // Assert
            Assert.AreEqual(1, client.Devices.Count);
            Assert.AreEqual("Test Device", client.Devices[0].Name);
        }

        [TestMethod]
        public void NetSdrClient_RemoveDevice_ShouldRemoveDevice()
        {
            // Arrange
            var client = new NetSdrClient();
            var device = new SDRDevice { Id = 1, Name = "Test Device" };
            client.AddDevice(device);

            // Act
            var result = client.RemoveDevice(1);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, client.Devices.Count);
        }
    }
}
