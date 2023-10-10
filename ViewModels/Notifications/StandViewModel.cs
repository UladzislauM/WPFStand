using TestStandApp.Buisness.Logger;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TestStandApp.ViewModels.Commands;
using TestStandApp.Buisness;

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
        private int _partImageWidth = 10;
        private int _partImageHeight = 704;
        private double _offsetX;
        private byte _checkingBytes;
        private Scenario _scenario;
        private ConsoleLogger _logger = new ConsoleLogger();

        public SingleCommandAsync ExecuteStartScan { get; private set; }
        public SingleCommand ExecuteStopScan { get; private set; }
        public SingleCommandAsync ExecuteStartScenario { get; private set; }
        public SingleCommand ExecuteStopScenario { get; private set; }

        private ObservableCollection<ImageSource> _imageCollection;

        public StandViewModel(Scenario scenario)
        {
            ExecuteStartScan = new SingleCommandAsync(ExecuteStartScanCommandAsync, CanExecute);
            ExecuteStopScan = new SingleCommand(ExecuteStopScanCommand, CanExecute);
            ExecuteStartScenario = new SingleCommandAsync(ExecuteStartScenarioCommandAsync, CanExecute);
            ExecuteStopScenario = new SingleCommand(ExecuteStopScenarioCommand, CanExecute);
            _scenario = scenario;
            _scenario.ImageBytesReceived += _scenarioImageBytesReceived;
            _scenario.ImageOffsetReceived += _scenarioImageOffsetReceived;
            _imageCollection = new ObservableCollection<ImageSource>();
        }

        public int StopByte
        {
            get => MyExtensions.StopScanNumber;
            set
            {
                MyExtensions.StopScanNumber = value;
                OnPropertyChanged(nameof(StopByte));
            }
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

        private void _scenarioImageOffsetReceived(double offset)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OffsetX -= offset / 2;
            });
        }

        public void AddImage(ImageSource image)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ImageCollection.Add(image);
            });
        }

        public void ExecuteStopScanCommand(object parameter)
        {
            MyExtensions.IsStartScan = false;
        }

        public async Task ExecuteStartScanCommandAsync()
        {
            MyExtensions.IsStartScan = true;

            ImageCollection.Clear();
            OffsetX = 0;

            Task.Run(() =>
            {
                _scenario.ScanToViewAsync(
                _selectedLocalSerialPortDetector,
                _selectedRemoteSerialPortDetector,
                _selectedAddressDetector,
                _partImageWidth,
                _partImageHeight);
            });
        }

        private void _scenarioImageBytesReceived(byte[] imageBytes)
        {
            try
            {
                ImageSource imageSource = MyExtensions.LoadImageFromBytes(imageBytes, _partImageWidth, _partImageHeight);

                AddImage(imageSource);
            }
            catch (Exception ex)
            {
                _logger.Log("event scenario image: " + ex.Message);
            }
        }

        public async Task ExecuteStartScenarioCommandAsync()
        {
            _isStartScenario = true;
            try
            {
                while (_isStartScenario)
                {
                    MyExtensions.IsStartScan = true;

                    await _scenario.RunScenarioAsync(
                                _selectedSerialPortBelt,
                                _selectedSerialPortGenerator,
                                _selectedLocalSerialPortDetector,
                                _selectedRemoteSerialPortDetector,
                                _selectedAddressDetector,
                                _partImageWidth,
                                _partImageHeight);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Stand program: " + ex.Message);
            }
        }

        private void ExecuteStopScenarioCommand(object parameter)
        {
            _isStartScenario = false;
            MyExtensions.IsStartScan = false;
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
