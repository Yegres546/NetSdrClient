using System.Collections.Generic;
using NetSdrClientApp.Models;

namespace NetSdrClientApp.Interfaces
{
    public interface IDeviceService
    {
        List<SDRDevice> Devices { get; }
        void AddDevice(SDRDevice device);
        bool RemoveDevice(int deviceId);
        SDRDevice? GetDevice(int deviceId);
        void Connect();
        void Disconnect();
    }
}
