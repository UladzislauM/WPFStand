using TestStandApp.Buisness.Logger;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Collections.Generic;
using TestStandApp.Connections;
using System;
using System.Collections;
using System.Linq;

namespace TestStandApp.Buisness.Equipment
{
    internal class Belt
    {
        private const byte StatusBeltTimeout = 100;
        public bool StartCheckStatus;
        public bool IsCorrectPlaceBelt { get; set; }
        public bool IsEndScenario { get; set; }
        private SerialPort? _port { get; set; }
        private Dictionary<string, string> _commandsDictionaryBelt;
        private readonly ILogger _logger;
        private readonly SerialPortConnection _serialPortConnection;

        public Belt(ILogger logger, SerialPortConnection serialPortConnection)
        {
            CreateTheDictoinaryWithCommands();
            _logger = logger;
            _serialPortConnection = serialPortConnection;
        }

        public void OpenPort(string port)
        {
            try
            {
                _port = _serialPortConnection.OpenPort(_port, port, 19200);
            }
            catch (Exception ex)
            {
                throw new Exception("Open port: " + ex.Message);
            }
        }

        public void ClosePort()
        {
            if (_port != null && _port.IsOpen)
            {
                StartCheckStatus = false;
                _port.Close();
                _port.Dispose();
            }
        }

        public async Task PrepareForUseAsync(string selectedPort)
        {
            try
            {
                if (_port == null || !_port.IsOpen)
                {
                    OpenPort(selectedPort);
                }

                await CheckForErrorsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Prepare for use: " + ex.Message);
            }
        }

        public async Task<byte[]> CheckStatusPLCAsync()
        {
            try
            {
                _logger.Log("Status");
                StartCheckStatus = true;
                return await ExecuteCommandAsync("Status");
            }
            catch (Exception ex)
            {
                throw new Exception("Check status PLC: " + ex.Message);
            }
        }

        public async Task CheckForErrorsAsync()
        {
            if (!_port?.IsOpen ?? false) // TODO will correct, add prepare for use
            {
                _logger.Log("Belt check for errors: The port is closet or lost!");
            }
            else
            {
                byte[] exceptionBytesPlatform = await ExecuteCommandAsync("Exceptions?");

                for (byte i = 1; i < 4; i++)
                {

                    BitArray exceptionBitesPlatform = MyExtensions.ByteForBites(exceptionBytesPlatform, i);

                    for (byte k = 0; k < 7; k++)
                    {
                        if (exceptionBitesPlatform[k])
                        {
                            _logger.Log("Belt check for errors: There is exception with platform - lets see logs!");
                        }
                    }
                }

                byte[] readStatusBelt = await CheckStatusPLCAsync();
                StartCheckStatus = true;
                BitArray statusBites = MyExtensions.ByteForBites(readStatusBelt, 2);

                if (statusBites[1])
                {
                    IsCorrectPlaceBelt = false;
                    _logger.Log("Belt check for errors: The belt situated on different place!");
                    await MoveBackAsync();
                }
            }
        }

        public async Task<byte[]> ExecuteCommandAsync(string command)
        {
            try
            {
                byte[] platformByteArray = CreateAByteCommand(command);
                return await _serialPortConnection.RunCommandStandAsync(_port, platformByteArray);
            }
            catch (Exception ex)
            {
                throw new Exception("Belt execute command: " + command + " " + ex.Message);
            }
        }

        public async Task MovingTheBeltAsync(string port, bool direction)
        {
            try
            {
                if (!StartCheckStatus)
                {
                    await PrepareForUseAsync(port);
                }

                if (direction)
                {
                    await MoveForwardAsync();
                }
                else
                {
                    await MoveBackAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Run belt: " + ex.Message);
            }
            finally
            {
                ShutDownConnectionToBelt();
            }
        }

        public void ShutDownConnectionToBelt()
        {
            if (IsEndScenario)
            {
                ClosePort();
            }
        }

        private async Task MoveForwardAsync()
        {
            byte[] readStatusBelt = await CheckStatusPLCAsync();
            BitArray statusBites = MyExtensions.ByteForBites(readStatusBelt, 2);

            if (statusBites[0])
            {
                await ExecuteCommandAsync("Working stroke of the platform with 30 speed");

                _logger.Log("Move forward: working stroke of the platform with 30 speed");

                await Task.Delay(StatusBeltTimeout);

                //off for emulator
                await CheckTheEndOfTheMovementAsync(1);
                IsCorrectPlaceBelt = false;
            }
            else
            {
                throw new Exception("Move forward: the platform situated on different place!");
            }
        }

        private async Task MoveBackAsync()
        {
            byte[] readStatusBelt = await CheckStatusPLCAsync();
            BitArray statusBites = MyExtensions.ByteForBites(readStatusBelt, 2);

            if (statusBites[1])
            {
                await ExecuteCommandAsync("Platform reverse with 600 speed");
                _logger.Log("Move back: platform reverse with 600 speed");

                await Task.Delay(StatusBeltTimeout);

                //off for emulator
                await CheckTheEndOfTheMovementAsync(0);
                IsCorrectPlaceBelt = true;
            }
            else
            {
                throw new Exception("Move back: The platform situated on the start place!");
            }

        }

        private async Task CheckTheEndOfTheMovementAsync(byte bitNumber)
        {
            byte[] readStatusBelt = await CheckStatusPLCAsync();
            BitArray statusBites = MyExtensions.ByteForBites(readStatusBelt, 2);

            byte checkMovePlatform = 0;
            while (!statusBites[bitNumber])
            {
                readStatusBelt = await CheckStatusPLCAsync();
                statusBites = MyExtensions.ByteForBites(readStatusBelt, 2);
                if (checkMovePlatform == 12)
                {
                    throw new Exception("Belt situated on the previous position");
                }

                checkMovePlatform++;
                await Task.Delay(1000); // TODO
            }
        }

        private void CreateTheDictoinaryWithCommands()
        {
            _commandsDictionaryBelt = new Dictionary<string, string>
            {
                { "Scan", "0xDC 0x04 0x28 0x02 0x058" },
                { "Starting position", "0xDC 0x02 0x2D" },
                { "Working stroke of the platform with 200 speed", "0xDC 0x04 0x01 0x00 0xC8" },
                { "Working stroke of the platform with 30 speed", "0xDC 0x04 0x01 0x00 0x96" },// 150
                { "Platform reverse with 600 speed", "0xDC 0x04 0x02 0x02 0x58" },
                { "Shutter on", "0xDC 0x04 0x03 0x01 0x00" },
                { "Shutter off", "0xDC 0x04 0x04 0x01 0x00" },
                { "Status", "0xDC 0x02 0x0B" },
                { "Work?", "0xDC 0x04 0x1B 0x01 0x01" },
                { "Exceptions?", "0xDC 0x02 0x0A" },
                { "WatchDog", "0xDC 0x04 0x11 0x00 0x01" }
            };
        }

        private byte[] CreateAByteCommand(string stringCommand)
        {
            string command = _commandsDictionaryBelt[stringCommand];
            byte[] byteArray = command.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
            byteArray = byteArray.Concat(new byte[] { CalculateLRC(byteArray) }).ToArray();
            return byteArray;
        }

        private byte CalculateLRC(byte[] data)
        {
            int sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum -= data[i];
            }
            byte crc = (byte)(sum);

            return crc;
        }
    }
}
