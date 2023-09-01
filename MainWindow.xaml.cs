using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;


namespace TestStandApp
{
    public partial class MainWindow : Window
    {
        private Dictionary<String, String> commands = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            FillSerialPortComboBox();
            CreateCommandsForDictionary();
            AddDataToComboBox();
        }

        private void Test_send_Click(object sender, RoutedEventArgs e)
        {
            SerialPort port = new SerialPort(serialPortComboBox.SelectedItem.ToString() ?? "Empty port",
          19200, Parity.None, 8, StopBits.One);
            try
            {

                port.Open();

                byte[] outByteArray = ReadData();

                port.Write(outByteArray, 0, outByteArray.Length);

                byte[] inData = new byte[port.BytesToRead];
                port.Read(inData, 0, inData.Length);

                in_data.Text = ByteArrayToFormattedString(inData);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exxx:" + ex.Message);
            }
            finally
            {
                port.Close();
            }

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
            string ff = Entered_command.Text;
            if (Entered_command.Text.Equals("Enter command") || Entered_command.Text.Equals(" "))
            {
                string keysCommands = CommandsBox.SelectedItem.ToString() ?? "Empty";
                string command = commands[keysCommands];
                byte[] byteArray = command.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                byteArray = byteArray.Concat(new byte[] { CalculateLRC(byteArray) }).ToArray();
                return byteArray;
            }
            else
            {
                string input = Entered_command.Text;
                byte[] byteArray = input.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
                return byteArray;
            }
        }

        private void FillSerialPortComboBox()
        {
            string[] portNames = SerialPort.GetPortNames();
            foreach (string portName in portNames)
            {
                serialPortComboBox.Items.Add(portName);
            }
        }

        private void CreateCommandsForDictionary()
        {
            commands = new Dictionary<string, string>
            {
                { "Scan", "0xDC 0x04 0x28 0x02 0x058" },
                { "Starting position", "0xDC 0x02 0x2D" },
                { "Run with 200 speed", "0xDC 0x04 0x01 0x02 0x00" },
                { "Run with 0 speed", "0xDC 0x04 0x02 0x00 0x00" },
                { "Light on", "0xDC 0x04 0x03 0x01 0x00" },
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

        private void AddDataToComboBox()
        {
            string[] commandsNames = commands.Keys.ToArray();
            foreach (string commandName in commandsNames)
            {
                CommandsBox.Items.Add(commandName);
            }
        }

        private void Entered_command_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Entered_command.Text.Equals("Enter commnd"))
            {
                Entered_command.Clear();
            }
        }
    }
}
