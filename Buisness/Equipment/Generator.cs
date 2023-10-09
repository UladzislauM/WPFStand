
using TestStandApp.Buisness.Logger;
using System.Collections;
using System.IO.Ports;
using System.Text;
using System.Collections.Generic;
using TestStandApp.Connections;
using System;
using System.Threading.Tasks;

namespace TestStandApp.Buisness.Equipment
{
    internal class Generator
    {
        private const byte _xRayDisabledTimeout = 150;
        private const int WatchDogResetTimeout = 600;

        private SerialPort? port { get; set; }
        public bool IsWorkGenerator { get; set; }


        private Dictionary<string, string> _commandsDictionaryGenerator;
        private readonly SerialPortConnection _serialPortConnection;
        private readonly ILogger _logger;


        public Generator(SerialPortConnection serialPortConnection, ILogger logger)
        {
            CreateTheDictoinaryWithCommands();
            _serialPortConnection = serialPortConnection;
            _logger = logger;
        }

        public void PrepareForUse(string port)
        {
            try
            {
                if (this.port == null || !this.port.IsOpen)
                {
                    OpenPort(port);

                    CheckForErrors();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Prepare for work: " + ex.Message);
            }
        }

        public void OpenPort(string port)
        {
            try
            {
                this.port = _serialPortConnection.OpenPort(this.port, port, 19200);
            }
            catch (Exception ex)
            {
                throw new Exception("Open port: " + ex.Message);
            }
        }

        public void ClosePort()
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
                port.Dispose();
            }
        }

        private void CheckForErrors()
        {
            ExecuteCommand("Fault Clear");
            byte[] exceptionBytesGenerator = ExecuteCommand("Report Fault");

            for (byte i = 0; i < 17; i += 2) //TODO 
            {
                if (exceptionBytesGenerator[i] != (48))
                {
                    _logger.Log("Check statement: There is exception with Generator - lets see logs!");
                }
            }
        }

        public byte[] ExecuteCommand(string command)
        {
            try
            {
                byte[] generatorByteArray = CreateAByteCommand(command);
                return _serialPortConnection.RunCommandGenerator(port, generatorByteArray);
            }
            catch (Exception ex)
            {
                throw new Exception("Service platform: " + command + " " + ex.Message);
            }
        }

        public async Task StartAsync(string port)
        {
            if (IsWorkGenerator)
            {
                try
                {
                    _logger.Log("Generator operation =>");

                    if (this.port == null || !this.port.IsOpen)
                    {
                        PrepareForUse(port);
                    }

                    ExecuteCommand("Watch dog Enable"); //Only for stand

                    byte[] generatorRead = ExecuteCommand("X-Ray stat");
                    if (generatorRead[1].Equals(48))
                    {
                        byte[] wathDogStatus = ExecuteCommand("Watch dog status");
                        if (wathDogStatus[1].Equals(49))
                        {
                            Task watchDogResetTask = WatchDogResetAsync();
                            await TurnOnXRayAsync();
                        }
                        else
                        {
                            throw new Exception("The watch dog timer is off");
                        }

                    }
                    else
                    {
                        throw new Exception("The X-Ray is On");
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception("Generator Work: " + ex.Message);
                }
                finally
                {
                    ShutDown();
                }
            }
            else
            {
                _logger.Log("The generator swicher is off");
            }
        }

        public void ShutDown()
        {
            if (!IsWorkGenerator)
            {
                ClosePort();
            }
        }

        public async Task WatchDogResetAsync()
        {
            while (IsWorkGenerator)
            {
                ExecuteCommand("Watch dog Reset");
                await Task.Delay(WatchDogResetTimeout);
            }
        }

        public async Task TurnOnXRayAsync()
        {
            byte[] xRayRead;
            if (IsWorkGenerator)
            {
                ExecuteCommand("Current Voltage 500");
                xRayRead = ExecuteCommand("X-Ray Enable");
                if (xRayRead[5].Equals(48))
                {
                    throw new Exception("The X-Ray isn't on!!!");
                }
                while (IsWorkGenerator)
                {
                    await Task.Delay(_xRayDisabledTimeout);
                }
            }
            else
            {
                throw new Exception("xRay: the button isWorkGenerator torn off");
            }
            ExecuteCommand("Current Voltage 0");
            xRayRead = ExecuteCommand("X-Ray Disable");
            if (xRayRead[5].Equals(49))
            {
                throw new Exception("The X-Ray isn't off!!!");
            }
            _logger.Log("Shutting down the Generator X");
        }

        private void CreateTheDictoinaryWithCommands()
        {
            _commandsDictionaryGenerator = new Dictionary<string, string>
            {
                { "X-Ray stat", "\x02STAT\x0D" },
                { "Watch dog status", "\x02WSTAT\x0D" },
                { "Watch dog Enable", "\x02WDOG1\x0D" },
                { "Watch dog Disable", "\x02WDOG0\x0D" },
                { "Watch dog Reset", "\x02WDTE\x0D" },
                { "X-Ray Enable", "\x02\x45NBL1\x0D" },
                { "X-Ray Disable", "\x02\x45NBL0\x0D" },
                { "Current Voltage 500", "\x02\x43P0500\x0D" },
                { "Current Voltage 0", "\x02\x43P0000\x0D" },
                { "Fault Clear", "\x02\x43LR\x0D" },
                { "Report Fault", "\x02\x46LT\x0D" }
            };
        }

        private byte[] CreateAByteCommand(string stringCommand)
        {
            string command = _commandsDictionaryGenerator[stringCommand];
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            return commandBytes;
        }
    }
}
