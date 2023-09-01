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


        public ObservableCollection<string> serialPortNames = new ObservableCollection<string>();
        private Dictionary<string, string> commandsDictionary = new Dictionary<string, string>();

        public ObservableCollection<string> SerialPortNames
        {
            get { return serialPortNames; }
            set
            {
                serialPortNames = value;
                OnPropertyChanged("SerialPortNames");
            }
        }

        public string SelectedCommand
        {
            get { return selectedCommand; }
            set
            {
                selectedCommand = value;
                OnPropertyChanged("SelectedCommand");
                ExecuteCommand.RaiseCanExecuteChanged();
            }
        }

        public string ResultText
        {
            get { return resultText; }
            set
            {
                resultText = value;
                OnPropertyChanged("SelectedCommand");
            }
        }
        public string EnteredCommandText
        {
            get { return enteredCommandText; }
            set
            {
                enteredCommandText = value;
                OnPropertyChanged("EnteredCommandText");
            }
        }

        public Dictionary<string, string> CommandsDictionary
        {
            get { return commandsDictionary; }
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

        private void Execute(object parameter)
        {
            if (SelectedCommand == null)
                return;

            SerialPort port = new SerialPort(SelectedCommand ?? "Empty port",
          19200, Parity.None, 8, StopBits.One);
            try
            {
                port.Open();
                byte[] outByteArray = ReadData();
                port.Write(outByteArray, 0, outByteArray.Length);

                byte[] inData = new byte[port.BytesToRead];
                port.Read(inData, 0, inData.Length);

                ResultText = ByteArrayToFormattedString(inData);
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
            return SelectedCommand != null;
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

        private byte[] ReadData()
        {
            if (EnteredCommandText.Equals("Enter command") || EnteredCommandText.Equals(" "))
            {
                string keysCommands = SelectedCommand ?? "Empty command";
                string command = CommandsDictionary[keysCommands];
                byte[] byteArray = command.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                byteArray = byteArray.Concat(new byte[] { CalculateLRC(byteArray) }).ToArray();
                return byteArray;
            }
            else
            {
                string input = EnteredCommandText;
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
                { "Run with 200 speed", "0xDC 0x04 0x01 0x02 0x00" },
                { "Light on", "0xDC 0x04 0x03 0x01 0x00" },
                { "Status", "0xDC 0x02 0x0B" },
                { "Exceptions?", "0xDC 0x02 0x0A" }
            };
        }

        private byte CalculateLRC(byte[] data)
        {
            byte lrc = 0x00;
            foreach (byte b in data)
            {
                lrc ^= b;
            }
            return lrc;
        }
    }

}
