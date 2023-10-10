using TestStandApp.Buisness.Logger;
using System.Collections;
using TestStandApp.Connections;
using System.Threading.Tasks;
using System;
using TestStandApp.Buisness.Equipment;

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
            int imageHeight,
            string path)
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

                    Task checkingScanSensor = StartScanAsync(generatorPort,
                                                        detectorLocalPort,
                                                        detectorRemotePort,
                                                        detectorAddress,
                                                        imageWidth,
                                                        imageHeight,
                                                        path);

                    await _belt.MovingTheBeltAsync(beltPort, true);

                    _logger.Log("Step 3");

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
                _detector.TurnOffDetector();
            }
        }

        private async Task StartScanAsync(
            string generatorPort,
            int detectorLocalPort,
            int detectorRemotePort,
            string detectorAddress,
            int imageWidth,
            int imageHeight,
            string path)
        {
            bool startScan = await CheckScanSensorAsync();

            if (startScan && !_belt.IsEndScenario)
            {
                _logger.Log("Start scan scenario");

                _generator.IsWorkGenerator = true;

                Task generatorWorkTask = _generator.StartAsync(generatorPort);//TODO mb better stop the belt before start scan

                await _shutter.TurnOnAsync();

                byte[] preparedBytes = await _detector.ScanAsync(detectorAddress, detectorLocalPort, detectorRemotePort, imageWidth, imageHeight);

                _detector.SavePicture(preparedBytes, imageWidth, imageHeight, path);

                _generator.IsWorkGenerator = false;
                generatorWorkTask.Dispose();

                await _shutter.TurnOffShutterAsync();
            }
        }

        private async Task<bool> CheckScanSensorAsync()
        {
            _logger.Log("Start checking scan sensor");

            byte[] readStatusBelt = await _belt.CheckStatusPLCAsync();
            BitArray statusBites = Extentions.ByteForBites(readStatusBelt, 1);

            while (!statusBites[4])
            {
                await Task.Delay(20);
                readStatusBelt = await _belt.CheckStatusPLCAsync();
                statusBites = Extentions.ByteForBites(readStatusBelt, 1);
            }
            return true;
        }
    }
}
