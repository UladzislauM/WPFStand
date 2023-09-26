using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using TestStandApp.ViewModels.Notifications;

namespace TestStandApp.Buisness
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        private StandViewModel viewModel = new StandViewModel();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] imageBytes)
            {
                int width = viewModel.selectedImageWidth;
                int height = viewModel.selectedImageHeight;
                PixelFormat format = PixelFormats.Gray16;

                WriteableBitmap writeableBitmap = new WriteableBitmap(width,
                   height, 90, 102, format, null);
                Int32Rect rect = new Int32Rect(0, 0, width, height);
                int stride = (width * format.BitsPerPixel + 7) / 8;

                writeableBitmap.WritePixels(rect, imageBytes, stride, 0);

                return writeableBitmap;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
