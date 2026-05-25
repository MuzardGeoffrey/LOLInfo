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
            // Garde : chemin absent ou vide (ex : SpellViewModel.Empty() avant chargement).
            // On retourne null → WPF n'affiche rien, pas d'exception.
            if (value is not string imagePath || string.IsNullOrWhiteSpace(imagePath))
                return null!;

            if (parameter is not string imageType)
                return value;

            string prefix = "pack://application:,,,/resources/";

            try
            {
                return imageType switch
                {
                    // "passive" redirige vers spell/ depuis la refacto des icônes
                    "spell" or "passive" =>
                        new BitmapImage(new Uri($"{prefix}spell/{imagePath}")),

                    "miniature" =>
                        new BitmapImage(new Uri($"{prefix}champion/miniature/{imagePath}")),

                    _ => value
                };
            }
            catch (Exception ex)
            {
                // URI malformée ou ressource absente : on trace sans planter l'UI.
                System.Diagnostics.Debug.WriteLine(
                    $"[ImagePathConverter] Impossible de charger '{imagePath}' (type={imageType}) : {ex.Message}");
                return null!;
            }
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