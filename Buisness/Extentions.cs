using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace TestStandApp.Buisness
{
    internal static class Extentions
    {
        private const int DelayTimeout = 5000;

        public static BitArray ByteForBites(byte[] writeData, byte testByteNumber)
        {
            if (testByteNumber <= writeData.Length)
            {
                byte testByte = writeData[testByteNumber];
                BitArray bites = new BitArray(BitConverter.GetBytes(testByte).ToArray());

                return bites;
            }
            else
            {
                throw new Exception("Byte for bites: check number more than array lenth!");
            }
        }

        public static async Task DelayTimeoutAsync(Task task)
        {
            if (await Task.WhenAny(task, Task.Delay(DelayTimeout)) == task)
            {
                return;
            }
            else
            {
                throw new Exception("Delay timeout write async: the serial port unavailable");
            }
        }
    }
}
