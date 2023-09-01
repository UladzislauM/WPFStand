using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TestStandApp.Models;
using TestStandApp.ViewModels.Commands;

namespace TestStandApp.ViewModels.Notifications
{
    internal class StandViewModel : MainViewModel
    {
        private SerialPort port;
        private string selectedCommand;
        private string resultText;
        private string enteredCommandText;
        private string[] portNames;
        private bool isActiveButton;
        private string selectedSerialPort;


        public ObservableCollection<string> serialPortNames = new ObservableCollection<string>();
        private Dictionary<string, string> commandsDictionary = new Dictionary<string, string>();

        public string SelectedSerialPort
        {
            get => selectedSerialPort;
            set
            {
                selectedSerialPort = value;
                OnPropertyChanged("SelectedSerialPort");
            }
        }

        public ObservableCollection<string> SerialPortNames
        {
            get => serialPortNames;
            set
            {
                serialPortNames = value;
                OnPropertyChanged("SerialPortNames");
            }
        }

        public bool IsActiveButton
        {
            get => isActiveButton;
            set
            {
                isActiveButton = value;
                OnPropertyChanged("isActiveListBox");
            }
        }

        public string SelectedCommand
        {
            get => selectedCommand;
            set
            {
                selectedCommand = value;
                OnPropertyChanged("SelectedCommand");
                IsActiveButton = true;
            }
        }

        public string ResultText
        {
            get => resultText = "Resualt";
            set
            {
                resultText = value;
                OnPropertyChanged();
            }
        }
        public string EnteredCommandText
        {
            get => enteredCommandText = "Write command";
            set
            {
                enteredCommandText = value;
                OnPropertyChanged("EnteredCommandText");
            }
        }

        public Dictionary<string, string> CommandsDictionary
        {
            get => commandsDictionary;
            set
            {
                commandsDictionary = value;
                OnPropertyChanged("CommandsDictionary");
            }
        }

        public SingleCommand ExecuteCommand { get; private set; }

        public StandViewModel()
        {
            CreateCommandsForDictionary();
            FillSerialPortNames();
            ExecuteCommand = new SingleCommand(Execute, CanExecute);
        }

        public void Execute(object parameter)
        {
            if (selectedCommand == null)
                return;

            port = new SerialPort(selectedSerialPort ?? "Empty port",
          19200, Parity.None, 8, StopBits.One);
            try
            {
                port.Open();
                byte[] outByteArray = WriteData();
                port.Write(outByteArray, 0, outByteArray.Length);

                byte[] inData = new byte[port.BytesToRead];
                port.Read(inData, 0, inData.Length);

                string string16s = ByteArrayToFormattedString(inData);
                resultText = string16s;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exxx:" + ex.Message);
            }
            finally
            {
                port?.Close();
            }
        }

        private bool CanExecute(object parameter)
        {
            return selectedCommand != null;
        }

        private string ByteArrayToFormattedString(byte[] byteArray)
        {
            string formattedString = "";

            foreach (byte b in byteArray)
            {
                formattedString += "0x" + b.ToString("X2") + " ";
            }

            return formattedString;
        }

        private byte[] WriteData()
        {
            if (enteredCommandText.Equals("Write command") || enteredCommandText.Equals(" "))
            {
                string keysCommands = selectedCommand ?? "Empty command";
                string command = CommandsDictionary[keysCommands];
                byte[] byteArray = command.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                byteArray = byteArray.Concat(new byte[] { CalculateLRC(byteArray) }).ToArray();
                return byteArray;
            }
            else
            {
                string input = enteredCommandText;
                byte[] byteArray = input.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                return byteArray;
            }
        }

        private void FillSerialPortNames()
        {
            portNames = SerialPort.GetPortNames();
            foreach (string portName in portNames)
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
                { "Light on", "0xDC 0x04 0x03 0x01 0x00" },
                { "Light off", "0xDC 0x04 0x04 0x01 0x00" },
                { "Status", "0xDC 0x02 0x0B" },
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
    }

}
