using StandConsoleApp.Buisness.Logger;
using System.IO.Ports;
using StandConsoleApp.Buisness;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace TestStandApp.Connections
{
    internal class SerialPortConnection
    {
        public int DelayProcessorTimeout { get; set; }
        private const int QuantityTryPortRead = 6;
        private SemaphoreSlim _semaphore;
        private readonly ILogger _logger;

        public SerialPortConnection(ILogger logger)
        {
            _logger = logger;
            _semaphore = new SemaphoreSlim(1);
            DelayProcessorTimeout = 100;
        }

        public async Task<SerialPort> OpenPortAsync(SerialPort port, string serialPort, int speed)
        {
            await _semaphore.WaitAsync();
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
            finally
            {
                _semaphore.Release();
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

        public async Task<byte[]> RunCommandAsync(SerialPort port, byte[] writeData)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (port != null && port.IsOpen)
                {
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();

                    Task writeDataTask = WriteDataAsync(port, writeData);
                    await Extentions.DelayTimeoutAsync(writeDataTask);

                    await CheckReadBytesAsync(port);

                    byte[] readData = ReadData(port);

                    string hexString = ByteArrayToFormattedString(readData);
                    //_logger.Log("Read: " + hexString);

                    return readData;
                }
                else
                {
                    throw new Exception("Run Command - The port is closet!");
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

        private async Task WriteDataAsync(SerialPort port, byte[] writeData)
        {
            await Task.Run(() =>
            {
                port.Write(writeData, 0, writeData.Length);
            });
            //string hexString = ByteArrayToFormattedString(writeData);

            //_logger.Log("Write: " + hexString);
        }

        private byte[] ReadData(SerialPort port)
        {
            byte[] readData = new byte[port.BytesToRead];
            byte checkRead = 0;
            while (readData[0].Equals(0) && !readData[0].Equals(220)
                || readData[0].Equals(0) && !readData[0].Equals(2))
            {
                if (checkRead == QuantityTryPortRead)
                {
                    throw new Exception("Run Command - The data didn't read!");
                }

                //_logger.Log("Run Command - Try read bytes");
                port.Read(readData, 0, readData.Length);
                checkRead++;

                CheckReadData(readData, checkRead);
            }

            return readData;
        }

        private void CheckReadData(byte[] writeData, byte checkRead)
        {
            if (writeData[0].Equals(220) && checkRead.Equals(2) || writeData[0].Equals(2) && checkRead.Equals(2))
            {
                throw new Exception("Run Command - the read Data begin with a damage byte!");
            }
        }

        private async Task CheckReadBytesAsync(SerialPort port)
        {
            byte checkTryRead = 0;
            while (port.BytesToRead < 1)
            {
                if (checkTryRead == 10)
                {
                    throw new Exception("Run command - Empty data!");
                }
                await Task.Delay(DelayProcessorTimeout);
                checkTryRead++;
            }
        }

        public string ByteArrayToFormattedString(byte[] readData)
        {
            return BitConverter.ToString(readData).Replace("-", " 0x").Insert(0, "0x");
        }
    }
}
