namespace LOLInfo.Utils
{
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    using LOLInfo.IServices.Storage;

    using Microsoft.Extensions.DependencyInjection;

    // Pour résoudre l'instance App (racine de composition) et son IServiceProvider.
    using LOLInfo;

    /// <summary>
    /// Comportement attaché : charge la <see cref="Image.Source"/> via le cache
    /// disque (<see cref="IImageCacheService"/>), de façon asynchrone, sans bloquer l'UI.
    ///
    /// Usage XAML :
    ///   &lt;Image utils:CachedImage.Kind="{x:Static utils:ImageConstant.MINIATURE}"
    ///          utils:CachedImage.SourcePath="{Binding Champion.Image.Full}" /&gt;
    ///
    /// <c>SourcePath</c> = nom de fichier (ex : "Ahri.png") ; <c>Kind</c> = type d'image.
    /// Un jeton de génération évite d'afficher une image périmée si la case est
    /// recyclée (liste virtualisée) avant la fin du téléchargement.
    /// </summary>
    public static class CachedImage
    {
        public static readonly DependencyProperty SourcePathProperty =
            DependencyProperty.RegisterAttached(
                "SourcePath", typeof(string), typeof(CachedImage),
                new PropertyMetadata(null, OnChanged));

        public static readonly DependencyProperty KindProperty =
            DependencyProperty.RegisterAttached(
                "Kind", typeof(string), typeof(CachedImage),
                new PropertyMetadata(null, OnChanged));

        // Incrémenté à chaque changement : seule la dernière demande applique sa source.
        private static readonly DependencyProperty TokenProperty =
            DependencyProperty.RegisterAttached(
                "Token", typeof(int), typeof(CachedImage), new PropertyMetadata(0));

        public static void SetSourcePath(DependencyObject o, string? v) => o.SetValue(SourcePathProperty, v);
        public static string? GetSourcePath(DependencyObject o) => (string?)o.GetValue(SourcePathProperty);

        public static void SetKind(DependencyObject o, string? v) => o.SetValue(KindProperty, v);
        public static string? GetKind(DependencyObject o) => (string?)o.GetValue(KindProperty);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Image image) return;

            // Invalide tout chargement en cours (cases recyclées, changement de skin…).
            var token = (int)image.GetValue(TokenProperty) + 1;
            image.SetValue(TokenProperty, token);
            image.Source = null;

            var filename = GetSourcePath(image);
            var kind     = GetKind(image);
            if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(kind)) return;

            var url = BuildUrl(kind!, filename!);
            if (url is null) return;

            var cache = ResolveCache();
            if (cache is null) return;

            _ = LoadAsync(image, cache, url, token);
        }

        private static async Task LoadAsync(Image image, IImageCacheService cache, string url, int token)
        {
            try
            {
                var source = await cache.GetImageAsync(url);

                // Retour sur le thread UI (pas de ConfigureAwait) : n'applique la
                // source que si la case n'a pas été réaffectée entre-temps.
                if (source is not null && (int)image.GetValue(TokenProperty) == token)
                    image.Source = source;
            }
            catch
            {
                // Image laissée vide en cas d'échec (déjà journalisé par le service).
            }
        }

        private static string? BuildUrl(string kind, string filename) => kind switch
        {
            ImageConstant.SPELL     => DataDragonCdn.SpellUrl(filename),
            ImageConstant.PASSIVE   => DataDragonCdn.PassiveUrl(filename),
            ImageConstant.MINIATURE => DataDragonCdn.ChampionUrl(filename),
            ImageConstant.SKIN      => DataDragonCdn.SkinUrl(filename),
            _ => null,
        };

        private static IImageCacheService? ResolveCache() =>
            (Application.Current as App)?.Services.GetService<IImageCacheService>();
    }
}
