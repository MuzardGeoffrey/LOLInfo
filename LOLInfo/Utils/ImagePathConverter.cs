namespace LOLInfo.Utils
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Convertit un nom de fichier image (ex : "AhriQ.png") en <see cref="BitmapImage"/>
    /// chargée depuis le CDN DataDragon.
    ///
    /// Le <c>ConverterParameter</c> indique le type d'image via <see cref="ImageConstant"/> :
    ///   "spell"     → CDN /img/spell/{filename}
    ///   "passive"   → CDN /img/passive/{filename}
    ///   "miniature" → CDN /img/champion/{filename}
    ///
    /// La version du patch est lue depuis <see cref="DataDragonCdn.Version"/>,
    /// mise à jour automatiquement au démarrage par <c>PatchVersionService</c>.
    /// Aucune ressource locale n'est plus requise — nouveaux champions et skins
    /// apparaissent dès le prochain patch sans recompilation.
    /// </summary>
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Garde : chemin absent ou vide (ex : SpellViewModel.Empty() avant chargement).
            if (value is not string imagePath || string.IsNullOrWhiteSpace(imagePath))
                return null!;

            if (parameter is not string imageType)
                return value;

            try
            {
                string url = imageType switch
                {
                    ImageConstant.SPELL     => DataDragonCdn.SpellUrl(imagePath),
                    ImageConstant.PASSIVE   => DataDragonCdn.PassiveUrl(imagePath),
                    ImageConstant.MINIATURE => DataDragonCdn.ChampionUrl(imagePath),
                    ImageConstant.SKIN => DataDragonCdn.SkinUrl(imagePath),
                    _                       => string.Empty,
                };

                if (string.IsNullOrEmpty(url))
                    return null!;

                // BitmapCacheOption.OnLoad : télécharge immédiatement, met en cache,
                // libère le flux réseau — évite les fuites de connexion HTTP.
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource     = new Uri(url);
                bitmap.CacheOption   = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.EndInit();

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ImagePathConverter] Impossible de charger '{imagePath}' (type={imageType}) : {ex.Message}");
                return null!;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Constantes de type d'image utilisées comme <c>ConverterParameter</c> en XAML.
    /// </summary>
    public static class ImageConstant
    {
        public const string PASSIVE   = "passive";
        public const string SPELL     = "spell";
        public const string MINIATURE = "miniature";
        public const string SKIN = "skin";
    }
}
