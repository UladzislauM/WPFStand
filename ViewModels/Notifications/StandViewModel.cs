using TestStandApp.Buisness.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using TestStandApp.Buisness.Equipment;
using TestStandApp.Connections;
using TestStandApp.ViewModels.Commands;
using TestStandApp.Buisness;
using System.Linq;

namespace TestStandApp.ViewModels.Notifications
{
    internal class StandViewModel : MainViewModel
    {
        private bool _isStartScan;
        private bool _isStartScenario;
        private string _selectedSerialPortBelt = "COM12";
        private string _selectedSerialPortGenerator = "COM14";
        private int _selectedLocalSerialPortDetector = 4001;
        private int _selectedRemoteSerialPortDetector = 3000;
        private string _selectedAddressDetector = "127.0.0.1";
        private string _selectedPath = "D:\\Vlad_doc\\ADVIN\\ResponceImages\\output_image.jpg";
        private int _partImageWidth = 20;
        private int _partImageHeight = 704;
        private double _offsetX;
        private byte _checkingBytes;
        private Detector _detector;
        private Scenario _scenario;
        private ConsoleLogger _logger = new ConsoleLogger();
        private Channel<byte[]> _channelForPackets;
        public SingleCommandAsync ExecuteStartScan { get; private set; }
        public SingleCommand ExecuteStopScan { get; private set; }
        public SingleCommandAsync ExecuteStartScenario { get; private set; }
        public SingleCommand ExecuteStopScenario { get; private set; }

        private ObservableCollection<ImageSource> _imageCollection;

        public StandViewModel(Detector detector, Scenario scenario)
        {
            ExecuteStartScan = new SingleCommandAsync(ExecuteStartScanCommandAsync, CanExecute);
            ExecuteStopScan = new SingleCommand(ExecuteStopScanCommand, CanExecute);
            ExecuteStartScenario = new SingleCommandAsync(ExecuteStartScenarioCommandAsync, CanExecute);
            ExecuteStopScenario = new SingleCommand(ExecuteStopScenarioCommand, CanExecute);
            _detector = detector;
            _scenario = scenario;
            _imageCollection = new ObservableCollection<ImageSource>();
        }

        public double OffsetX
        {
            get => _offsetX;
            set
            {
                if (_offsetX != value)
                {
                    _offsetX = value;
                    OnPropertyChanged(nameof(OffsetX));
                }
            }
        }

        public byte CheckingBytes
        {
            get => _checkingBytes;
            set
            {
                _checkingBytes = value;
                OnPropertyChanged(nameof(CheckingBytes));
            }
        }

        public ObservableCollection<ImageSource> ImageCollection
        {
            get => _imageCollection;
            set
            {
                _imageCollection = value;
                OnPropertyChanged(nameof(ImageCollection));
            }
        }

        private void MoveItemsLeft()
        {
            OffsetX -= _partImageWidth / 2;
        }

        public void AddImage(ImageSource image)
        {
            ImageCollection.Add(image);
        }

        public void ExecuteStopScanCommand(object parameter)
        {
            _isStartScan = false;
        }

        public async Task ExecuteStartScanCommandAsync()
        {
            _isStartScan = true;
            await ScanToViewAsync();
        }

        private async Task ScanToViewAsync()
        {
            try
            {
                ImageCollection.Clear();
                OffsetX = 0;
                _checkingBytes = 0;

                _detector.StartScan(
                    _selectedAddressDetector,
                    _selectedLocalSerialPortDetector,
                    _selectedRemoteSerialPortDetector);
                //byte checkingBytes = 0;
                byte stop = 250;

                _channelForPackets = Channel.CreateUnbounded<byte[]>();
                Task writeBytes = Task.Run(() => { WriteBytesToChannel(stop); });

                await Task.Delay(300);

                byte[] bytesFromChannel;

                ImageSource imageSource;

                while (_checkingBytes != stop)
                {
                    if (_isStartScan)
                    {
                        bytesFromChannel = await _channelForPackets.Reader.ReadAsync();
                        imageSource = Extentions.LoadImageFromBytes(bytesFromChannel, _partImageWidth, _partImageHeight);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AddImage(imageSource);
                        });

                        if (_checkingBytes >= 20)
                        {
                            MoveItemsLeft();
                        }
                        CheckingBytes++;
                    }
                    else
                    {
                        writeBytes.Dispose();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Execute: " + ex.Message);
            }
            finally
            {
                await _detector.StopScan();
                _channelForPackets.Writer.Complete();
            }
        }

        public async Task ExecuteStartScenarioCommandAsync()
        {
            _isStartScenario = true;
            try
            {
                while (_isStartScenario)
                {
                    Console.WriteLine("Ready for button");
                    await _scenario.RunScenarioAsync(
                                _selectedSerialPortBelt,
                                _selectedSerialPortGenerator,
                                _selectedLocalSerialPortDetector,
                                _selectedRemoteSerialPortDetector,
                                _selectedAddressDetector,
                                _partImageWidth,
                                _partImageHeight,
                                _selectedPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Stand program: " + ex.Message);
            }
        }

        private void ExecuteStopScenarioCommand(object parameter)
        {
            _isStartScenario = false;
        }

        private async Task WriteBytesToChannel(byte stop)
        {
            byte checkingBytes = 0;
            byte[] preparedBytes;
            while (checkingBytes != stop)
            {
                preparedBytes = await _detector.ScanAsync(
                    _selectedAddressDetector,
                    _selectedLocalSerialPortDetector,
                    _selectedRemoteSerialPortDetector,
                    _partImageWidth,
                    _partImageHeight);

                await _channelForPackets.Writer.WriteAsync(preparedBytes);

                checkingBytes++;
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
