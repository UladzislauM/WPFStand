using System;
using System.Collections;
using System.Threading.Tasks;

namespace TestStandApp.Buisness.Equipment
{
    internal class Button
    {
        private const byte _keyForStart = 5;// for stand 5, for Emulator 0(Encoder1)
        //public bool IsRunPlatform { get; set; }
        private readonly Belt _belt;

        public Button(Belt belt)
        {
            _belt = belt;
        }

        public async Task<bool> CheckScenarioKeyAsync(string port)
        {
            try
            {
                if (!_belt.StartCheckStatus)
                {
                    await _belt.PrepareForUseAsync(port);
                }
                byte[] readStatusBelt;

                while (true)
                {
                    readStatusBelt = await _belt.CheckStatusPLCAsync();
                    BitArray statusBites = Extentions.ByteForBites(readStatusBelt, 1);
                    if (statusBites[_keyForStart])
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Check scenario key: " + ex.Message);
            }
        }
    }
}
