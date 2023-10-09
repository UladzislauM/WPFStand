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
using TestStandApp.Infrastructure.Commands;

namespace TestStandApp.ViewModels.Notifications
{
    internal class StandViewModel : MainViewModel
    {
        private bool _isStartScan;
        private int _selectedLocalSerialPortDetector = 4001;
        private int _selectedRemoteSerialPortDetector = 3000;
        private string _selectedAddressDetector = "127.0.0.1";
        private int _selectedImageWidth = 20;
        private int _selectedImageHeight = 704;
        private double _offsetX;
        private byte _checkingBytes;
        private Detector _detector;
        private ConsoleLogger _logger = new ConsoleLogger();
        private Channel<byte[]> _channelForPackets;
        public SingleCommandAsync ExecuteStartScan { get; private set; }
        public SingleCommand ExecuteStopScan { get; private set; }
        public SingleCommand ExecuteStartScenario { get; private set; }

        private ObservableCollection<ImageSource> _imageCollection;

        public StandViewModel()
        {
            ExecuteStartScan = new SingleCommandAsync(ExecuteStartScanCommandAsync, CanExecute);
            ExecuteStopScan = new SingleCommand(ExecuteStopScanCommand, CanExecute);
            ExecuteStartScenario = new SingleCommand(ExecuteStartScenarioCommand, CanExecute);
            _detector = new Detector(new LanConnection(_logger), _logger);
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

        private void MoveItemsLeft()
        {
            OffsetX -= _selectedImageWidth / 2;
        }

        public void AddImage(ImageSource image)
        {
            ImageCollection.Add(image);
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

        public void ExecuteStopScanCommand(object parameter)
        {
            _isStartScan = false;
        }
        
        public async Task ExecuteStartScanCommandAsync()
        {
            try
            {
                _isStartScan = true;
                ImageCollection.Clear();
                OffsetX = 0;
                _checkingBytes = 0;

                await _detector.StartScan(
                    _selectedAddressDetector,
                    _selectedLocalSerialPortDetector,
                    _selectedRemoteSerialPortDetector);
                //byte checkingBytes = 0;
                byte stop = 250;

                Task writeBytes = WriteBytesToChannel(stop);

                byte[] bytesFromChannel;

                ImageSource imageSource;

                while (_checkingBytes != stop)
                {
                    if (_isStartScan)
                    {
                        bytesFromChannel = await _channelForPackets.Reader.ReadAsync();
                        imageSource = LoadImageFromBytes(bytesFromChannel);

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

        public void ExecuteStartScenarioCommand(object parameter)
        {
            _isStartScan = false;
        }

        private async Task WriteBytesToChannel(byte stop)
        {
            _channelForPackets = Channel.CreateUnbounded<byte[]>();
            byte checkingBytes = 0;
            byte[] preparedBytes;
            while (checkingBytes != stop)
            {
                preparedBytes = await _detector.ScanAsync(
                _selectedImageWidth,
                _selectedImageHeight);

                await _channelForPackets.Writer.WriteAsync(preparedBytes);

                checkingBytes++;
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        public ImageSource LoadImageFromBytes(byte[] imageBytes)
        {
            try
            {
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    throw new Exception("Load image from bytes: Empty bytes");
                }

                int width = _selectedImageWidth;
                int height = _selectedImageHeight;
                PixelFormat format = PixelFormats.Gray16;

                WriteableBitmap writeableBitmap = new WriteableBitmap(width,
                   height, 96, 96, format, null);
                Int32Rect rect = new Int32Rect(0, 0, width, height);
                int stride = (width * format.BitsPerPixel + 7) / 8;

                writeableBitmap.WritePixels(rect, imageBytes, stride, 0);

                return writeableBitmap;

            }
            catch (Exception ex)
            {
                throw new Exception("Load image from bytes: " + ex.Message);
            }
        }
    }
}
