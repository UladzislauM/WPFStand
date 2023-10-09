using TestStandApp.Buisness.Logger;
using System.Collections;
using TestStandApp.Connections;

namespace TestStandApp.Buisness
{
    internal class Scenario
    {
        //private readonly SerialPortConnection _serialPortConnection;
        //private readonly ILogger _logger;
        //private readonly Belt _belt;
        //private readonly Generator _generator;
        //private readonly Shutter _shutter;
        //private readonly Button _button;
        //private readonly Detector _detector;

        //public Scenario(SerialPortConnection repository,
        //    ILogger logger,
        //    Belt belt,
        //    Generator generator,
        //    Shutter shutter,
        //    Button button,
        //    Detector detector)
        //{
        //    _serialPortConnection = repository;
        //    _logger = logger;
        //    _belt = belt;
        //    _generator = generator;
        //    _shutter = shutter;
        //    _button = button;
        //    _detector = detector;
        //}

        //public async Task RunLineProgramAsync(
        //    string beltPort,
        //    string generatorPort,
        //    int detectorLocalPort,
        //    int detectorRemotePort,
        //    string detectorAddress,
        //    int imageWidth,
        //    int imageHeight,
        //    string path)
        //{

        //    try
        //    {
        //        await _button.CheckScenarioKey(beltPort);

        //        if (_button.IsRunPlatform)
        //        {
        //            _belt.IsEndScenario = false;

        //            await _generator.PrepareForUseAsync(generatorPort);
        //            await _belt.PrepareForUseAsync(beltPort);
        //            await _detector.PrepareForUseAsync(detectorAddress, detectorLocalPort, detectorRemotePort);

        //            Task runBelt = _belt.MovingTheBeltAsync(beltPort, true);

        //            BitArray statusBites = Extentions.ByteForBites(_belt.ReadStatusBelt, 3);

        //            _logger.Log("Start checking scan sensor");
        //            byte readingAttempt = 0;
        //            while (!statusBites[4] || readingAttempt == 15)
        //            {
        //                await Task.Delay(300);
        //                statusBites = Extentions.ByteForBites(_belt.ReadStatusBelt, 3);
        //                readingAttempt++;
        //            }

        //            if (statusBites[4])
        //            {
        //                _logger.Log("Start scan scenario");

        //                _generator.IsWorkGenerator = true;

        //                Task generatorWorkTask = _generator.StartAsync(generatorPort);

        //                await _shutter.TurnOnAsync();

        //                byte[] preparedBytes = await _detector.ScanAsync(detectorAddress, detectorLocalPort, detectorRemotePort, imageWidth, imageHeight);

        //                _detector.SavePicture(preparedBytes, imageWidth, imageHeight, path);

        //                _generator.IsWorkGenerator = false;

        //                await _shutter.TurnOffAsync();
        //            }

        //            await _belt.MovingTheBeltAsync(beltPort, false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Run scenario: " + ex.Message);
        //    }
        //    finally
        //    {
        //        _generator.IsWorkGenerator = false;
        //        await _shutter.TurnOffAsync();
        //        _button.IsRunPlatform = false;
        //        _belt.IsEndScenario = true;
        //        _detector.TurnOff();
        //    }
        //}
    }
}
