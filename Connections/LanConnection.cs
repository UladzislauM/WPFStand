using SixLabors.ImageSharp.Formats.Jpeg;
using TestStandApp.Buisness.Logger;
using System;
using System.Net;
using System.Net.Sockets;

namespace TestStandApp.Connections
{
    internal class LanConnection
    {
        private const int ReceivingTimeout = 3000;
        public UdpClient UdpReceiver;
        public TcpClient TcpClient;
        private NetworkStream _stream;
        private IPEndPoint _ipEndPoint;

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
                _ipEndPoint = new IPEndPoint(ipAddress, localPort);
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

        public byte[] ExecuteCommand(byte[] data)
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

        public byte[] ReceiveAMessage()
        {
            try
            {
                byte[] receiveResualt = UdpReceiver.Receive(ref _ipEndPoint);
                return receiveResualt;
            }
            catch (Exception ex)
            {
                throw new Exception("Receiving message: " + ex.Message);
            }
        }
    }
}
