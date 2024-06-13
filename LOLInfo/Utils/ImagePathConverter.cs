namespace LOLInfo.Utils
{
    using System.Globalization;
    using System.Windows.Media.Imaging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;

    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath)
            {
                // Ajoutez ici le préfixe ou le suffixe au chemin de l'image
                string prefix = "pack://application:,,,/resources/champion/miniature/"; // Exemple de préfixe pour les ressources intégrées
                return new BitmapImage(new Uri($"{prefix}{imagePath}"));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}