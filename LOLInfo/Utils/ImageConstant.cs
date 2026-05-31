namespace LOLInfo.Utils
{
    /// <summary>
    /// Types d'image, utilisés comme <c>CachedImage.Kind</c> en XAML pour
    /// déterminer le dossier CDN à interroger.
    /// </summary>
    public static class ImageConstant
    {
        public const string PASSIVE   = "passive";
        public const string SPELL     = "spell";
        public const string MINIATURE = "miniature";
        public const string SKIN      = "skin";
    }
}
