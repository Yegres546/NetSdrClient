using System;
using System.Threading.Tasks;

namespace NetSdrClientApp.Interfaces
{
    public interface IUdpClient : IDisposable
    {
        event EventHandler<byte[]>? MessageReceived;
        Task StartListeningAsync();
        void StopListening();
        void Exit();
    }
}
