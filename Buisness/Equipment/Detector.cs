using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using StandConsoleApp.Buisness.Logger;
using TestStandApp.Connections;

namespace TestStandApp.Buisness.Equipment
{
    internal class Detector
    {
        private byte[] _data;
        private readonly LanConnection _lanConnection;
        private readonly ILogger _logger;
        private Dictionary<string, string> _commandsDictionaryDetector;
        private Channel<byte[]> _channelForReadBytes;

        public Detector(LanConnection lanConnection, ILogger logger)
        {
            _lanConnection = lanConnection;
            _logger = logger;
            CreateTheDictoinaryWithCommands();
        }

        public async Task PrepareForUseAsync(string address, int localPort, int remotePort)
        {
            try
            {
                _lanConnection.SetUpAConnection(address, localPort, remotePort);
                await ExecuteCommandAsync("Set the integration time");

                _logger.Log("Detector: Prepared for use");
            }
            catch (Exception ex)
            {
                throw new Exception("Preparing for use: " + ex.Message);
            }
        }

        public void TurnOff()
        {
            _lanConnection.ClosePorts();
        }

        public async Task<byte[]?> ExecuteCommandAsync(string command)
        {
            try
            {
                _logger.Log("Execute command");

                byte[] data = CreateAByteCommand(command);
                byte[] receivingData = await _lanConnection.ExecuteCommandAsync(data);

                if (receivingData[0] == 0)
                {
                    throw new Exception("The read data is empty");
                }

                return receivingData;
            }
            catch (Exception ex)
            {
                throw new Exception("Execute command async: " + ex.Message);
            }
        }

        public async Task StartScan(string address,
            int localPort,
            int remotePort)
        {
            try
            {
                if (_lanConnection.TcpClient == null || _lanConnection.UdpReceiver == null)
                {
                    await PrepareForUseAsync(address, localPort, remotePort);
                    _logger.Log("Prepared for use.");
                }
                await ExecuteCommandAsync("Start scan");
            }
            catch (Exception ex)
            {
                throw new Exception("Start scan: " + ex.Message);
            }
        }

        public async Task<byte[]> ScanAsync(
            int imageWidth,
            int imageHeight)
        {
            try
            {
                int packets = imageWidth * 2;

                _logger.Log("Receiving");

                byte[] rawBytes = await GetBytesAsync(packets, imageHeight);

                _logger.Log("Scan OK");
                return rawBytes;
            }
            catch (Exception ex)
            {
                throw new Exception($"Scan: " + ex.Message);
            }
        }

        public async Task StopScan()
        {
            try
            {
                _logger.Log("Scan OK");
                await ExecuteCommandAsync("Stop scan");
            }
            catch (Exception ex)
            {
                throw new Exception($"Stop scan: " + ex.Message);
            }
        }

        private async Task<byte[]> GetBytesAsync(int packets, int imageHeight)
        {
            int packetsRow = 0;
            int imageArrayNumber = 0;
            Task writeBytesForChannel = WriteBytesIntoChannel(packets);

            byte[] imageArrayByte = new byte[(packets * imageHeight)];
            int position = 0;
            while (packetsRow != packets)
            {
                try
                {
                    _data = await _channelForReadBytes.Reader.ReadAsync();
                    int i = 0;
                    if (CheckTheHeader(_data))
                    {
                        i = 8;
                    }

                    for (; i < _data.Length - 1; i++)
                    {
                        if (imageArrayNumber != imageHeight)
                        {
                            position = packetsRow + imageArrayNumber * packets;
                            if (imageArrayNumber > 511)
                            {
                                position--;
                            }
                            imageArrayByte[position] = _data[i++];
                            imageArrayByte[position + 1] = _data[i];
                            imageArrayNumber++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (imageArrayNumber == imageHeight)
                    {
                        imageArrayNumber = 0;
                    }
                    packetsRow++;
                    if (packetsRow == 80)
                    {
                        Console.WriteLine();
                    }
                    _logger.Log("Read" + packetsRow);
                }
                catch (Exception ex)
                {
                    writeBytesForChannel.Dispose();
                    await ExecuteCommandAsync("Stop scan");
                    throw new Exception("Getting bytes: " + ex.Message);
                }
            }
            writeBytesForChannel.Dispose();
            return imageArrayByte;
        }

        private async Task WriteBytesIntoChannel(int packets)
        {
            _channelForReadBytes = Channel.CreateUnbounded<byte[]>();
            byte checkingBytes = 0;
            byte[] preparedBytes;
            while (checkingBytes != packets)
            {
                preparedBytes = await _lanConnection.ReceiveAMessageAsync();

                await _channelForReadBytes.Writer.WriteAsync(preparedBytes);

                checkingBytes++;
            }
        }

        private static bool CheckTheEnergy(byte[] data, bool isHeader)
        {
            bool checkEnergy = false;
            if (isHeader)
            {
                if (data[3] == 8)
                {
                    checkEnergy = true;
                }
            }

            return checkEnergy;
        }

        private static bool CheckTheHeader(byte[] data)
        {
            bool isHeader = false;
            if (data[0] == 235 && data[1] == 144 & data[2] == 0)
            {
                isHeader = true;
            }

            return isHeader;
        }

        public void SavePicture(byte[] preparedBytes, int imageWidth, int imageHeight, string path)
        {
            var finalImage = Image.LoadPixelData<L16>(preparedBytes, imageWidth, imageHeight);

            finalImage.Save(path, new JpegEncoder());
            finalImage.Dispose();
            _logger.Log("Saving OK");
        }

        private void CreateTheDictoinaryWithCommands()
        {
            _commandsDictionaryDetector = new Dictionary<string, string>
            {
                { "Start scan", "[SF,1]" },
                { "Stop scan", "[SF,0]" },
                { "Set the integration time", "[ST,W,1388]" }
            };
        }

        private byte[] CreateAByteCommand(string stringCommand)
        {
            string command = _commandsDictionaryDetector[stringCommand];
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            return commandBytes;
        }
    }
}
