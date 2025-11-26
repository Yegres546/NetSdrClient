using NetSdrClientApp.Messages;
using NetSdrClientApp.Networking;
using NetSdrClientApp.Models;
using NetSdrClientApp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NetSdrClientApp.Services
{
    public class SDRClient : IDeviceService, IDisposable
    {
        public List<SDRDevice> Devices { get; private set; }
        private bool _disposed = false;

        public SDRClient()
        {
            Devices = new List<SDRDevice>();
        }

        public void Connect()
        {
            // Connection logic
            Console.WriteLine("SDRClient connecting...");
        }

        public void Disconnect()
        {
            // Disconnection logic
            Console.WriteLine("SDRClient disconnecting...");
        }

        public void AddDevice(SDRDevice device)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            Devices.Add(device);
        }

        public bool RemoveDevice(int deviceId)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var device = Devices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                Devices.Remove(device);
                return true;
            }
            return false;
        }

        public SDRDevice? GetDevice(int deviceId)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Devices.FirstOrDefault(d => d.Id == deviceId);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cleanup managed resources
                    Devices.Clear();
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

namespace NetSdrClientApp
{
    public class NetSdrClient : IDisposable
    {
        private readonly ITcpClient _tcpClient;
        private readonly IUdpClient _udpClient;
        private TaskCompletionSource<byte[]>? _responseTaskSource;
        private bool _disposed = false;

        public bool IQStarted { get; set; }

        public NetSdrClient(ITcpClient tcpClient, IUdpClient udpClient)
        {
            _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _udpClient = udpClient ?? throw new ArgumentNullException(nameof(udpClient));

            _tcpClient.MessageReceived += TcpClient_MessageReceived;
            _udpClient.MessageReceived += UdpClient_MessageReceived;
        }

        public async Task ConnectAsync()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_tcpClient.Connected)
            {
                _tcpClient.Connect();

                var sampleRate = BitConverter.GetBytes((long)100000).Take(5).ToArray();
                var automaticFilterMode = BitConverter.GetBytes((ushort)0).ToArray();
                var adMode = new byte[] { 0x00, 0x03 };

                // Host pre setup
                var msgs = new List<byte[]>
                {
                    NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.IQOutputDataSampleRate, sampleRate),
                    NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.RFFilter, automaticFilterMode),
                    NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.ADModes, adMode),
                };

                foreach (var msg in msgs)
                {
                    await SendTcpRequest(msg);
                }
            }
        }

        public void Disconnect()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _tcpClient.Disconnect();
        }

        public async Task StartIQAsync()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_tcpClient.Connected)
            {
                Console.WriteLine("No active connection.");
                return;
            }

            var iqDataMode = (byte)0x80;
            var start = (byte)0x02;
            var fifo16bitCaptureMode = (byte)0x01;
            var n = (byte)1;

            var args = new[] { iqDataMode, start, fifo16bitCaptureMode, n };

            var msg = NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.ReceiverState, args);

            await SendTcpRequest(msg);

            IQStarted = true;

            _ = _udpClient.StartListeningAsync();
        }

        public async Task StopIQAsync()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_tcpClient.Connected)
            {
                Console.WriteLine("No active connection.");
                return;
            }

            var stop = (byte)0x01;

            var args = new byte[] { 0, stop, 0, 0 };

            var msg = NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.ReceiverState, args);

            await SendTcpRequest(msg);

            IQStarted = false;

            _udpClient.StopListening();
        }

        public async Task ChangeFrequencyAsync(long hz, int channel)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_tcpClient.Connected)
            {
                Console.WriteLine("No active connection.");
                return;
            }

            var channelArg = (byte)channel;
            var frequencyArg = BitConverter.GetBytes(hz).Take(5);
            var args = new[] { channelArg }.Concat(frequencyArg).ToArray();

            var msg = NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.ReceiverFrequency, args);

            await SendTcpRequest(msg);
        }

        private void UdpClient_MessageReceived(object? sender, byte[] e)
        {
            if (_disposed) return;

            try
            {
                NetSdrMessageHelper.TranslateMessage(e, out MsgTypes type, out ControlItemCodes code, out ushort sequenceNum, out byte[] body);
                var samples = NetSdrMessageHelper.GetSamples(16, body);

                Console.WriteLine($"Samples received: " + body.Select(b => Convert.ToString(b, toBase: 16)).Aggregate((l, r) => $"{l} {r}"));

                using (FileStream fs = new FileStream("samples.bin", FileMode.Append, FileAccess.Write, FileShare.Read))
                using (BinaryWriter sw = new BinaryWriter(fs))
                {
                    foreach (var sample in samples)
                    {
                        sw.Write((short)sample); // write 16 bit per sample as configured 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing UDP message: {ex.Message}");
            }
        }

        private async Task<byte[]?> SendTcpRequest(byte[] msg)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_tcpClient.Connected)
            {
                Console.WriteLine("No active connection.");
                return null;
            }

            _responseTaskSource = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            var responseTask = _responseTaskSource.Task;

            await _tcpClient.SendMessageAsync(msg);

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
            var completedTask = await Task.WhenAny(responseTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _responseTaskSource.TrySetCanceled();
                Console.WriteLine("TCP request timeout.");
                return null;
            }

            return await responseTask;
        }

        private void TcpClient_MessageReceived(object? sender, byte[] e)
        {
            if (_disposed) return;

            try
            {
                // TODO: add Unsolicited messages handling here
                if (_responseTaskSource != null && !_responseTaskSource.Task.IsCompleted)
                {
                    _responseTaskSource.TrySetResult(e);
                    _responseTaskSource = null;
                }
                Console.WriteLine("Response received: " + e.Select(b => Convert.ToString(b, toBase: 16)).Aggregate((l, r) => $"{l} {r}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing TCP message: {ex.Message}");
                _responseTaskSource?.TrySetException(ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cleanup managed resources
                    _tcpClient.MessageReceived -= TcpClient_MessageReceived;
                    _udpClient.MessageReceived -= UdpClient_MessageReceived;
                    
                    _tcpClient.Dispose();
                    _udpClient.Dispose();
                    
                    _responseTaskSource?.TrySetCanceled();
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
