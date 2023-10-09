using TestStandApp.Buisness.Logger;
using System.Collections;
using System.Threading.Tasks;

namespace TestStandApp.Buisness.Equipment
{
    internal class Shutter
    {
        private readonly ILogger _logger;
        private readonly Belt _belt;

        public Shutter(ILogger logger, Belt belt)
        {
            _logger = logger;
            _belt = belt;
        }

        public async Task TurnOnAsync()
        {
            byte[] sutterReadBytes = await _belt.ExecuteCommandAsync("Shutter on");

            BitArray shutterBites = Extentions.ByteForBites(sutterReadBytes, 0);

            if (!shutterBites[1])
            {
                _logger.Log("Shutter on: the shutter didn't open!");
            }
            _logger.Log("Shutter on");
        }

        public async Task TurnOffShutterAsync()
        {
            byte[] sutterReadBytes = await _belt.ExecuteCommandAsync("Shutter off");

            BitArray shutterBites = Extentions.ByteForBites(sutterReadBytes, 0);

            if (!shutterBites[2])
            {
                _logger.Log("Shutter on: The shutter didn't CLOSE!");
            }
            _logger.Log("Shutter off");
        }
    }
}
