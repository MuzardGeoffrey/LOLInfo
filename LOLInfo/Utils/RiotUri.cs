namespace LOLInfo.Utils
{
    /// <summary>
    /// Fournit les URL de l'API DataDragon pour les données de jeu.
    ///
    /// Les URL sont construites dynamiquement depuis <see cref="DataDragonCdn.Version"/>,
    /// laquelle est mise à jour au démarrage par <c>PatchVersionService</c>.
    ///
    /// Plus besoin de modifier ce fichier à chaque nouveau patch.
    /// </summary>
    public static class RiotUri
    {
        /// <summary>URL de la liste complète des champions (fr_FR).</summary>
        public static string GENERAL() => DataDragonCdn.GeneralDataUrl();

        /// <summary>URL du détail d'un champion spécifique.</summary>
        public static string DETAIL(string championName) => DataDragonCdn.ChampionDetailUrl(championName);
    }
}
