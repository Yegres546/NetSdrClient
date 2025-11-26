using System;
using System.Threading.Tasks;

namespace NetSdrClientApp.Interfaces
{
    public interface ITcpClient : IDisposable
    {
        bool Connected { get; }
        event EventHandler<byte[]>? MessageReceived;
        void Connect();
        void Disconnect();
        Task SendMessageAsync(byte[] data);
        Task SendMessageAsync(string str);
    }
}
