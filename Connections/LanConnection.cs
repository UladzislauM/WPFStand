using SixLabors.ImageSharp.Formats.Jpeg;
using StandConsoleApp.Buisness.Logger;
using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestStandApp.Connections
{
    internal class LanConnection
    {
        private const int ReceivingTimeout = 3000;
        public UdpClient UdpReceiver;
        public TcpClient TcpClient;
        private NetworkStream _stream;
        private UdpReceiveResult _udpReceiveResult;
        CancellationTokenSource _cancellationTokenSource;

        private readonly ILogger _logger;

        public LanConnection(ILogger logger)
        {
            _logger = logger;
        }

        public void SetUpAConnection(string address, int localPort, int remotePort)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(address);
                UdpReceiver = new UdpClient(localPort);
                TcpClient = new TcpClient(address, remotePort);
                _stream = TcpClient.GetStream();
            }
            catch (Exception ex)
            {
                throw new Exception("ConnectAsync: " + ex.Message);
            }
        }

        public void ClosePorts()
        {
            if (UdpReceiver != null)
            {
                UdpReceiver.Close();
                UdpReceiver.Dispose();

                TcpClient.Close();
                TcpClient.Dispose();
            }
        }

        public async Task<byte[]> ExecuteCommandAsync(byte[] data)
        {
            byte[] readData = new byte[16];
            try
            {
                _stream.Write(data, 0, data.Length);

                _stream.Read(readData, 0, readData.Length);
            }
            catch (Exception ex)
            {
                throw new Exception("Execute command: " + ex.Message);
            }
            return readData;
        }

        public async Task<byte[]> ReceiveAMessageAsync()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationTokenSource.CancelAfter(ReceivingTimeout);
                _udpReceiveResult = await UdpReceiver.ReceiveAsync(_cancellationTokenSource.Token);
                return _udpReceiveResult.Buffer;
            }
            catch (OperationCanceledException operationException)
            {
                throw new Exception("Receiving message: Temeout receiving.");
            }
            catch (Exception ex)
            {
                throw new Exception("Receiving message: " + ex.Message);
            }
        }
    }
}
