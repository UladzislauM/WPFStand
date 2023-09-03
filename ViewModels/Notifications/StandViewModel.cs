using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TestStandApp.Models;
using TestStandApp.ViewModels.Commands;

namespace TestStandApp.ViewModels.Notifications
{
    internal class StandViewModel : MainViewModel
    {
        private const int MillisecondsTimeout = 1000;
        private const int QualityTryLineProgram = 10;
        private const int QuantityTryPortRead = 6;
        private string _filePath = @"C:\LogsStandTest.txt";
        private string _testFilePath = @"C:\LogsStandTestTest.txt";
        private SerialPort _port;
        private string _selectedCommand;
        private string _resultText;
        private string _enteredCommandText;
        private string[] _portNames;
        private bool _isActiveButton;
        private string _selectedSerialPort;
        private bool _cicleCommand;
        private bool _openPort;


        private ObservableCollection<string> _serialPortNames = new ObservableCollection<string>();
        private Dictionary<string, string> _commandsDictionary = new Dictionary<string, string>();

        public StandViewModel()
        {
            CreateCommandsForDictionary();
            FillSerialPortNames();
            ExecuteCommand = new SingleCommand(Execute, CanExecute);
        }

        public bool OpenPort
        {
            get => _openPort;
            set
            {
                _openPort = value;
                OnPropertyChanged(nameof(OpenPort));
                ChangeButton();
                OpenPortForProgram();
            }
        }

        public bool CicleCommand
        {
            get => _cicleCommand;
            set
            {
                _cicleCommand = value;
                OnPropertyChanged(nameof(CicleCommand));
                ChangeButton();
                RunLineProgram();
            }
        }

        public string SelectedSerialPort
        {
            get => _selectedSerialPort;
            set
            {
                _selectedSerialPort = value;
                OnPropertyChanged(nameof(SelectedSerialPort));
            }
        }

        public ObservableCollection<string> SerialPortNames
        {
            get => _serialPortNames;
            set
            {
                _serialPortNames = value;
                OnPropertyChanged("SerialPortNames"); //Fix
            }
        }

        public bool IsActiveButton
        {
            get => _isActiveButton;
            set
            {
                _isActiveButton = value;
                OnPropertyChanged("isActiveListBox");
            }
        }

        public string SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                _selectedCommand = value;
                OnPropertyChanged("SelectedCommand");
                IsActiveButton = true;
            }
        }

        public string ResultText
        {
            get => _resultText = "Resualt";
            set
            {
                _resultText = value;
                OnPropertyChanged();
            }
        }
        public string EnteredCommandText
        {
            get => _enteredCommandText = "Write command";
            set
            {
                _enteredCommandText = value;
                OnPropertyChanged("EnteredCommandText");
            }
        }

        public Dictionary<string, string> CommandsDictionary
        {
            get => _commandsDictionary;
            set
            {
                _commandsDictionary = value;
                OnPropertyChanged("CommandsDictionary");
            }
        }

        public SingleCommand ExecuteCommand { get; private set; }

        public void Execute(object parameter)
        {
            if (_selectedCommand == null)
                return;

            byte[] outByteArray = WriteData();

            RunCommand(outByteArray, _filePath);
        }

        private void OpenPortForProgram()
        {
            if (_openPort)
            {
                _port = new SerialPort(_selectedSerialPort ?? "Empty port",
             19200, Parity.None, 8, StopBits.One);
                try
                {
                    _port.WriteTimeout = 500;
                    _port.ReadTimeout = 500;
                    _port.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exxx:" + ex.Message);
                }
            }
            else
            {
                ClosePort();
            }
        }

        public void ClosePort()
        {
            if (_port != null && _port.IsOpen)
            {
                _port.Close();
                _port.Dispose();
            }
        }

        private void RunLineProgram()
        {
            byte[] statusByteArray = new byte[] { 220, 2, 11, 23 };
            bool isKeyHold = false;
            byte check = 0;

            while (_cicleCommand)
            {
                if (check == QualityTryLineProgram)
                {
                    _cicleCommand = false;
                }

                byte[] testInData = RunCommand(statusByteArray, _testFilePath);

                byte testByte = testInData[3];
                char[] biteMask = new char[8];
                for (int i = 0; i < 8; i++)
                {
                    biteMask[i] = (testByte & (1 << i)) == 0 ? '0' : '1';
                }

                if (biteMask[1].Equals('1'))
                {
                    isKeyHold = true;
                    break;
                }
                check++;
            }

            if (isKeyHold)//check each time
            {
                CurtainOnOff(true);

                RunPlatform(true);//reverse byte array 

                RunPlatform(false);

                CurtainOnOff(false);

                isKeyHold = false;
                _cicleCommand = false;
            }
        }

        private void CurtainOnOff(bool curtainIsOn)
        {
            byte[] curtain;

            if (curtainIsOn)
            {
                curtain = DictinaryToTheCommand("Curtain on");
            }
            else
            {
                curtain = DictinaryToTheCommand("Curtain off");
            }

            byte[] executedCommand = RunCommand(curtain, _testFilePath);

            byte testByte = executedCommand[2];
            char[] biteMask = new char[8];
            for (int i = 0; i < 8; i++)
            {
                biteMask[i] = (testByte & (1 << i)) == 0 ? '0' : '1';
            }

            if (!biteMask[1].Equals('1') && curtainIsOn)
            {
                MessageBox.Show("Exxx: The curtain didn't open!!!");
            }

            if (!biteMask[2].Equals('1') && !curtainIsOn)
            {
                MessageBox.Show("Exxx: The curtain didn't CLOSE!!!");
            }
        }

        private void RunPlatform(bool direction)
        {
            byte[] platform = new byte[] { };
            if (direction)
            {
                platform = DictinaryToTheCommand("Working stroke of the platform with 200 speed");
            }
            else
            {
                platform = DictinaryToTheCommand("Platform reverse with 600 speed");
            }

            byte[] statusByteArray = new byte[] { 220, 2, 11, 23 };

            byte[] testInData = RunCommand(statusByteArray, _testFilePath);

            byte testByte = testInData[3];
            char[] biteMask = new char[8];
            for (int i = 0; i < 8; i++)
            {
                biteMask[i] = (testByte & (1 << i)) == 0 ? '0' : '1';
            }

            if (biteMask[1].Equals('1') && direction)
            {
                byte[] executedCommand = RunCommand(platform, _testFilePath);

                byte testBytePlatform = executedCommand[2];
                char[] biteMaskPlatform = new char[8];
                for (int i = 0; i < 8; i++)
                {
                    biteMaskPlatform[i] = (testBytePlatform & (1 << i)) == 0 ? '0' : '1';
                }

                if (!biteMaskPlatform[0].Equals('1'))
                {
                    MessageBox.Show("Exxx: The platform didn't run!!!");
                }
            }
            else
            {
                MessageBox.Show("Exxx: The platform situated on different place!");
            }

            if (biteMask[0].Equals('1') && !direction)
            {
                byte[] executedCommand = RunCommand(platform, _testFilePath);

                byte testBytePlatform = executedCommand[2];
                char[] biteMaskPlatform = new char[8];
                for (int i = 0; i < 8; i++)
                {
                    biteMaskPlatform[i] = (testBytePlatform & (1 << i)) == 0 ? '0' : '1';
                }

                if (!biteMaskPlatform[6].Equals('1'))
                {
                    MessageBox.Show("Exxx: The platform didn't run!!!");
                }
            }
        }

        private byte[] RunCommand(byte[] command, string filePath)
        {
            if (_port != null && _port.IsOpen)
            {
                _port.DiscardInBuffer();
                _port.DiscardOutBuffer();

                LoggsToFile("In", _selectedCommand, filePath);
                _port.Write(command, 0, command.Length);

                Thread.Sleep(MillisecondsTimeout);

                byte[] testInData = new byte[_port.BytesToRead];
                byte checkRead = 0;

                while (testInData.Length!=0 && testInData[0] == 0)
                {
                    if (checkRead == QuantityTryPortRead)
                    {
                        MessageBox.Show("Exxx: The data didn't read!");
                        break;
                    }

                    _port.Read(testInData, 0, testInData.Length);
                    checkRead++;
                }



                string testString16s = ByteArrayToFormattedString(testInData);
                LoggsToFile("Out", testString16s, filePath);

                return testInData;
            }
            else
            {
                MessageBox.Show("Exxx: The port is closet!");
                return new byte[0];
            }
        }

        private bool CanExecute(object parameter)
        {
            return _selectedCommand != null;
        }

        private string ByteArrayToFormattedString(byte[] byteArray)
        {
            string formattedString = "";

            foreach (byte b in byteArray)
            {
                formattedString += "0x" + b.ToString("X2") + " ";
            }

            return formattedString.Trim();
        }

        private byte[] WriteData()
        {
            if (_enteredCommandText.Equals("Write command") || _enteredCommandText.Equals(" "))
            {
                string keysCommands = _selectedCommand ?? "Empty command";
                byte[] byteArray = DictinaryToTheCommand(keysCommands);
                return byteArray;
            }
            else
            {
                string input = _enteredCommandText;
                byte[] byteArray = input.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                return byteArray;
            }
        }

        private byte[] DictinaryToTheCommand(string keysCommands)
        {
            string command = CommandsDictionary[keysCommands];
            byte[] byteArray = command.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
            byteArray = byteArray.Concat(new byte[] { CalculateLRC(byteArray) }).ToArray();
            return byteArray;
        }

        private void FillSerialPortNames()
        {
            _portNames = SerialPort.GetPortNames();
            foreach (string portName in _portNames)
            {
                SerialPortNames.Add(portName);
            }
        }

        private void CreateCommandsForDictionary()
        {
            CommandsDictionary = new Dictionary<string, string>
            {
                { "Scan", "0xDC 0x04 0x28 0x02 0x058" },
                { "Starting position", "0xDC 0x02 0x2D" },
                { "Working stroke of the platform with 200 speed", "0xDC 0x04 0x01 0x00 0xC8" },
                { "Platform reverse with 600 speed", "0xDC 0x04 0x02 0x02 0x58" },
                { "Curtain on", "0xDC 0x04 0x03 0x01 0x00" },
                { "Curtain off", "0xDC 0x04 0x04 0x01 0x00" },
                { "Status", "0xDC 0x02 0x0B" },
                { "Work?", "0xDC 0x04 0x00 0x1B" },
                { "Exceptions?", "0xDC 0x02 0x0A" }
            };
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

        private void LoggsToFile(string who, string log, string path)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(who + " " + log);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exxx:" + ex.Message);
            }
        }

        private void ChangeButton()
        {
            if (_cicleCommand == true)
            {
                _cicleCommand = true;
            }
            else
            {
                _cicleCommand = false;
            }
        }
    }
}
