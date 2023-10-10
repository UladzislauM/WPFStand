using TestStandApp.Buisness.Logger;
using System.Collections;
using TestStandApp.Connections;
using System.Threading.Tasks;
using System;
using TestStandApp.Buisness.Equipment;
using System.Threading.Channels;
using System.Windows.Media;
using System.Windows;

namespace TestStandApp.Buisness
{
    internal class Scenario
    {
        private readonly ILogger _logger;
        private readonly Belt _belt;
        private readonly Generator _generator;
        private readonly Shutter _shutter;
        private readonly Button _button;
        private readonly Detector _detector;
        bool _isRunPlatform;
        private Channel<byte[]> _channelForPackets;
        public event Action<byte[]> ImageBytesReceived;
        public event Action<double> ImageOffsetReceived;

        public Scenario(ILogger logger,
            Belt belt,
            Generator generator,
            Shutter shutter,
            Button button,
            Detector detector)
        {
            _logger = logger;
            _belt = belt;
            _generator = generator;
            _shutter = shutter;
            _button = button;
            _detector = detector;
        }

        public async Task RunScenarioAsync(
            string beltPort,
            string generatorPort,
            int detectorLocalPort,
            int detectorRemotePort,
            string detectorAddress,
            int imageWidth,
            int imageHeight)
        {

            try
            {
                _isRunPlatform = await _button.CheckScenarioKeyAsync(beltPort);
                _logger.Log("Step 1");

                if (_isRunPlatform)
                {
                    _belt.IsEndScenario = false;

                    _generator.PrepareForUse(generatorPort);
                    _detector.PrepareForUse(detectorAddress, detectorLocalPort, detectorRemotePort);
                    await _belt.PrepareForUseAsync(beltPort);

                    _logger.Log("Step 2");

                    Task.Run(() =>
                    {
                        StartScanAsync(generatorPort,
                                       detectorLocalPort,
                                       detectorRemotePort,
                                       detectorAddress,
                                       imageWidth,
                                       imageHeight);
                    });

                    await _belt.MovingTheBeltAsync(beltPort, true);

                    _logger.Log("Step 3");

                    MyExtensions.IsStartScan = false;

                    await _belt.MovingTheBeltAsync(beltPort, false);
                    _logger.Log("Step 4");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Run scenario: " + ex.Message);
            }
            finally
            {
                _generator.IsWorkGenerator = false;
                //await _shutter.TurnOffShutter();
                _isRunPlatform = false;
                _belt.IsEndScenario = true;
                _belt.ShutDownConnectionToBelt();
                _detector.TurnOffDetector();//TODO
            }
        }

        private async Task StartScanAsync(
            string generatorPort,
            int detectorLocalPort,
            int detectorRemotePort,
            string detectorAddress,
            int imageWidth,
            int imageHeight)
        {
            bool startScan = await CheckScanSensorAsync();

            if (startScan && !_belt.IsEndScenario)
            {
                _logger.Log("Start scan scenario");

                _generator.IsWorkGenerator = true;

                Task generatorWorkTask = Task.Run(() =>
                {
                    _generator.StartAsync(generatorPort);//TODO mb better stop the belt before start scan
                });

                await _shutter.TurnOnAsync();

                await ScanToViewAsync(
                    detectorLocalPort,
                    detectorRemotePort,
                    detectorAddress,
                    imageWidth,
                    imageHeight);

                _generator.IsWorkGenerator = false;

                await _shutter.TurnOffShutterAsync();
                //generatorWorkTask.Dispose();
            }
        }

        public async Task ScanToViewAsync(
            int detectorLocalPort,
            int detectorRemotePort,
            string detectorAddress,
            int imageWidth,
            int imageHeight)
        {
            try
            {
                _detector.StartScan(
                   detectorAddress,
                   detectorLocalPort,
                   detectorRemotePort);

                byte checkingBytes = 0;

                _channelForPackets = Channel.CreateUnbounded<byte[]>();
                Task writeBytes = Task.Run(() => { WriteBytesToChannel(
                   detectorAddress,
                   detectorLocalPort,
                   detectorRemotePort,
                   imageWidth,
                   imageHeight); });

                await Task.Delay(300);

                byte[] bytesFromChannel;

                ImageSource imageSource;

                while (checkingBytes != MyExtensions.StopScanNumber)
                {
                    if (MyExtensions.IsStartScan)
                    {
                        bytesFromChannel = await _channelForPackets.Reader.ReadAsync();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ImageBytesReceived.Invoke(bytesFromChannel);
                        });

                        if (checkingBytes >= 20)
                        {
                            ImageOffsetReceived.Invoke(imageWidth);
                        }
                        checkingBytes++;
                    }
                    else
                    {
                        //writeBytes.Dispose();
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

        private async Task WriteBytesToChannel(
            string detectorAddress,
            int detectorLocalPort,
            int detectorRemotePort,
            int imageWidth,
            int imageHeight)
        {
            byte checkingBytes = 0;
            byte[] preparedBytes;
            while (checkingBytes != MyExtensions.StopScanNumber)
            {
                preparedBytes = await _detector.ScanAsync(
                   detectorAddress,
                   detectorLocalPort,
                   detectorRemotePort,
                   imageWidth,
                   imageHeight);

                await _channelForPackets.Writer.WriteAsync(preparedBytes);

                checkingBytes++;
            }
        }

        private async Task<bool> CheckScanSensorAsync()
        {
            _logger.Log("Start checking scan sensor");

            byte[] readStatusBelt = await _belt.CheckStatusPLCAsync();
            BitArray statusBites = MyExtensions.ByteForBites(readStatusBelt, 1);

            while (!statusBites[4])
            {
                await Task.Delay(20);
                readStatusBelt = await _belt.CheckStatusPLCAsync();
                statusBites = MyExtensions.ByteForBites(readStatusBelt, 1);
            }
            return true;
        }
    }
}
