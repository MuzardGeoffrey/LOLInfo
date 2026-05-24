namespace LOLInfo.Utils
{
    using System.Globalization;
    using System.Windows.Media.Imaging;
    using System;
    using System.Windows.Data;

    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath)
            {
                string prefix = "pack://application:,,,/resources/";
                if (parameter is string imageType)
                {
                    switch (imageType)
                    {
                        case "spell":
                            return new BitmapImage(new Uri($"{prefix}spell/{imagePath}"));
                            break;

                        case "passive":
                            return new BitmapImage(new Uri($"{prefix}passive/{imagePath}"));
                            break;

                        case "miniature":
                            return new BitmapImage(new Uri($"{prefix}champion/miniature/{imagePath}"));

                        default:
                            return value;
                            break;
                    }
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class ImageConstant
    {
        public static string PASSIVE = "passive";

        public static string SPELL = "spell";

        public static string MINIATURE = "miniature";
    }
}