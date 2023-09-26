using StandConsoleApp.Buisness.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TestStandApp.Buisness.Equipment;
using TestStandApp.Connections;
using TestStandApp.ViewModels.Commands;

namespace TestStandApp.ViewModels.Notifications
{
    internal class StandViewModel : MainViewModel
    {
        private int selectedLocalSerialPortDetector = 4001;
        private int selectedRemoteSerialPortDetector = 3000;
        private string selectedAddressDetector = "127.0.0.1";
        public int selectedImageWidth = 10;
        public int selectedImageHeight = 704;
        private byte[] _imageBytes;
        private Detector _detector;
        private ConsoleLogger _logger = new ConsoleLogger();
        private Queue<byte[]> _queue;
        private Channel<byte[]> _channel;
        public AsyncSingleCommand StartScan { get; private set; }

        private ObservableCollection<ImageSource> _imageCollection;

        public StandViewModel()
        {
            StartScan = new AsyncSingleCommand(ExecuteAsync, CanExecute);
            _detector = new Detector(new LanConnection(_logger), _logger);
            _queue = new Queue<byte[]>();
            _imageCollection = new ObservableCollection<ImageSource>();
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

        //public byte[] ImageBytes
        //{
        //    get => _imageBytes;
        //    set
        //    {
        //        _imageBytes = value;
        //        OnPropertyChanged(nameof(ImageBytes));
        //    }
        //}

        public async Task ExecuteAsync()
        {
            try
            {
                _channel = Channel.CreateUnbounded<byte[]>();

                await _detector.StartScan(
                    selectedAddressDetector,
                    selectedLocalSerialPortDetector,
                    selectedRemoteSerialPortDetector);
                byte checkingBytes = 0;
                byte stop = 20;

                Task writeBytes = WriteBytesToQueue(stop);

                byte[] bytesFromChannel;

                ImageSource imageSource;

                while (checkingBytes != stop)
                {
                    bytesFromChannel = await _channel.Reader.ReadAsync();
                    imageSource = LoadImageFromBytes(bytesFromChannel);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AddImage(imageSource);
                    });
                    checkingBytes++;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Execute: " + ex.Message);
            }
            finally
            {
                await _detector.StopScan();
                _channel.Writer.Complete();
            }
        }

        private async Task WriteBytesToQueue(byte stop)
        {
            byte checkingBytes = 0;
            byte[] preparedBytes;
            while (checkingBytes != stop)
            {
                preparedBytes = await _detector.ScanAsync(
                selectedImageWidth,
                selectedImageHeight);

                await _channel.Writer.WriteAsync(preparedBytes);

                checkingBytes++;
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        public void AddImage(ImageSource image)
        {
            _imageCollection.Add(image);
        }

        public ImageSource LoadImageFromBytes(byte[] imageBytes)
        {
            try
            {
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    throw new Exception("Load image from bytes: Empty bytes");
                }

                int width = selectedImageWidth;
                int height = selectedImageHeight;
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
