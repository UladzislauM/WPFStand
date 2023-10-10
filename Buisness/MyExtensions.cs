using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace TestStandApp.Buisness
{
    internal static class MyExtensions
    {
        public static bool IsStartScan;
        public static int StopScanNumber = 86;
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

        public static ImageSource LoadImageFromBytes(byte[] imageBytes, int width, int height) //TODO add extention method
        {
            try
            {
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    throw new Exception("Load image from bytes: Empty bytes");
                }

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
