using TestStandApp.Buisness.Logger;
using System.IO.Ports;
using TestStandApp.Buisness;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace TestStandApp.Connections
{
    internal class SerialPortConnection
    {
        public int DelayProcessorTimeout { get; set; }
        private int _firstByte = 0;
        private SemaphoreSlim _semaphore;
        private readonly ILogger _logger;

        public SerialPortConnection(ILogger logger)
        {
            _logger = logger;
            _semaphore = new SemaphoreSlim(1);
            DelayProcessorTimeout = 100;
        }

        public SerialPort OpenPort(SerialPort port, string serialPort, int speed)
        {
            try
            {
                port = new SerialPort(serialPort ?? "Empty port", speed, Parity.None, 8, StopBits.One);

                port.Open();
                return port;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Opan port for device: {0}, {1}", port.ToString, ex.Message));
            }
        }

        public void ClosePort(SerialPort port)
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
                port.Dispose();
            }
        }

        public async Task<byte[]> RunCommandStandAsync(SerialPort port, byte[] writeData)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (port != null && port.IsOpen)
                {
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();

                    port.Write(writeData, 0, writeData.Length);

                    byte[] readData = ReadDataStand(port);

                    string hexString = ByteArrayToFormattedString(readData);
                    _logger.Log("Read: " + hexString);

                    return readData;
                }
                else
                {
                    throw new Exception("Run Command async - The port is closet!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Run command async: " + ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private byte[] ReadDataStand(SerialPort port)
        {
            Task.Run(() => { Task delayReadByte = DelayRead(); });
            _firstByte = port.ReadByte();
            if (_firstByte > 0)
            {
                byte attemptsReadByte = 0;
                while (_firstByte != 220)
                {
                    _firstByte = port.ReadByte();
                    if (attemptsReadByte == 15)
                    {
                        throw new Exception("Read data stand: Bytes incorrect.");
                    }
                    attemptsReadByte++;
                }

                byte[] readDataHeader = new byte[port.BytesToRead];
                port.Read(readDataHeader, 0, readDataHeader.Length);

                if (readDataHeader.Length > 1)
                {
                    int dataLength = Convert.ToInt16(readDataHeader[0]);
                    if (readDataHeader.Length - 1 == dataLength)
                    {
                        byte[] correctBytes = new byte[dataLength];
                        Array.Copy(readDataHeader, 1, correctBytes, 0, correctBytes.Length);

                        return correctBytes;
                    }
                    else
                    {
                        byte[] readData = ReadBytesFromQueue(port, dataLength);
                        return readData;
                    }
                }
                else
                {
                    port.Read(readDataHeader, 1, readDataHeader.Length - 1);
                    int dataLength = readDataHeader[1];
                    byte[] readdata = ReadBytesFromQueue(port, dataLength);
                    return readdata;
                }
            }
            else
            {
                throw new Exception("Read data stand: connection lost!");
            }
        }


        private byte[] ReadBytesFromQueue(SerialPort port, int dataLength)
        {
            byte[] readData = new byte[dataLength];
            for (int i = 0; i < readData.Length - 1; i++)
            {
                port.Read(readData, i, dataLength - i); //check reading byte
            }

            return readData;
        }

        public byte[] RunCommandGenerator(SerialPort port, byte[] writeData)
        {
            try
            {
                if (port != null && port.IsOpen)
                {
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();

                    port.Write(writeData, 0, writeData.Length);

                    byte[] readData = ReadDataGenerator(port);

                    string hexString = ByteArrayToFormattedString(readData);
                    _logger.Log("Read: " + hexString);

                    return readData;
                }
                else
                {
                    throw new Exception("Run Command generator - The port is closet!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Run command generator: " + ex.Message);
            }
        }

        private byte[] ReadDataGenerator(SerialPort port)
        {
            int firstByte = port.ReadByte();
            if (firstByte > 0)
            {
                byte attemptsReadByte = 0;
                while (firstByte != 2)
                {
                    firstByte = port.ReadByte();
                    if (attemptsReadByte == 15)
                    {
                        throw new Exception("Read data generator: Bytes incorrect.");
                    }
                    attemptsReadByte++;
                }

                byte dataArraySize = 20;
                byte[] readData = new byte[dataArraySize];
                readData[0] = (byte)firstByte;
                byte iteration = 1;
                while (readData[iteration] == 13)
                {
                    iteration++;
                    if (readData.Length == iteration)
                    {
                        Array.Resize(ref readData, dataArraySize + 10);
                    }
                    readData[iteration] = (byte)port.ReadByte();
                }
                return readData;
            }
            else
            {
                throw new Exception("Read data generator: connection lost!");
            }
        }

        public string ByteArrayToFormattedString(byte[] readData)
        {
            return BitConverter.ToString(readData).Replace("-", " 0x").Insert(0, "0x");
        }

        private async Task DelayRead()
        {
            await Task.Delay(DelayProcessorTimeout);
            if (_firstByte == 0)
            {
                _logger.Log("Read data stand: connection lost!");
            }
        }
    }
}
