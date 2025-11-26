using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class UdpClientWrapper : IUdpClient, IDisposable
{
    private readonly IPEndPoint _localEndPoint;
    private CancellationTokenSource _cts;
    private UdpClient? _udpClient;
    private bool _disposed = false;

    public event EventHandler<byte[]>? MessageReceived;

    public UdpClientWrapper(int port)
    {
        _localEndPoint = new IPEndPoint(IPAddress.Any, port);
        _cts = new CancellationTokenSource();
    }

    public async Task StartListeningAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Console.WriteLine("Start listening for UDP messages...");

        try
        {
            _udpClient = new UdpClient(_localEndPoint);
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
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
            }
        }
    }

    public void StopListening()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            _cts?.Cancel();
            _udpClient?.Close();
            Console.WriteLine("Stopped listening for UDP messages.");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Console.WriteLine($"Error while stopping: {ex.Message}");
            }
        }
    }

    public void Exit()
    {
        // Exit - це по суті те саме що StopListening, але для узгодженості
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
                // Dispose managed resources
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
