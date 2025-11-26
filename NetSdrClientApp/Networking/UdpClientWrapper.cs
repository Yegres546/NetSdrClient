using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{
    public class UdpClientWrapper : IUdpClient, IDisposable
    {
        private readonly IPEndPoint _localEndPoint;
        private CancellationTokenSource _cts;
        private UdpClient? _udpClient;
        private bool _disposed = false;
        private bool _isListening = false;

        public event EventHandler<byte[]>? MessageReceived;

        public UdpClientWrapper(int port)
        {
            _localEndPoint = new IPEndPoint(IPAddress.Any, port);
            _cts = new CancellationTokenSource();
        }

        public async Task StartListeningAsync()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_isListening)
            {
                Console.WriteLine("Already listening for UDP messages.");
                return;
            }

            Console.WriteLine("Start listening for UDP messages...");

            try
            {
                _udpClient = new UdpClient(_localEndPoint);
                _isListening = true;
                
                while (!_cts.Token.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync(_cts.Token);
                    MessageReceived?.Invoke(this, result.Buffer);
                    Console.WriteLine($"Received from {result.RemoteEndPoint}");
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (ObjectDisposedException)
            {
                // Expected when UdpClient is disposed
            }
            catch (SocketException ex)
            {
                // Socket-specific errors
                Console.WriteLine($"Socket error: {ex.SocketErrorCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                // Only log other exceptions if we're not disposed and were actually listening
                Console.WriteLine($"Error receiving message: {ex.Message}");
            }
            finally
            {
                _isListening = false;
                Console.WriteLine("Stopped listening for UDP messages.");
            }
        }

        public void StopListening()
        {
            if (_disposed) return;

            try
            {
                _cts?.Cancel();
                _udpClient?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while stopping: {ex.Message}");
            }
        }

        public void Exit()
        {
            StopListening();
        }

        public override int GetHashCode()
        {
            var payload = $"{nameof(UdpClientWrapper)}|{_localEndPoint.Address}|{_localEndPoint.Port}";

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(payload));

            return BitConverter.ToInt32(hash, 0);
        }

        private void CleanupResources()
        {
            try
            {
                _isListening = false;
                _cts?.Cancel();
                _udpClient?.Close();
                _udpClient?.Dispose();
                _udpClient = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CleanupResources();
                    _cts?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
