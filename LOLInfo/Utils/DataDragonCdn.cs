namespace LOLInfo.Utils
{
    using LOLInfo.Localization;

    /// <summary>
    /// Constructeur d'URL pour le CDN DataDragon de Riot.
    ///
    /// La version est initialisée au démarrage par <c>PatchVersionService.InitializeAsync()</c>.
    /// Les converters et helpers lisent <see cref="Version"/> directement — aucun DI requis.
    ///
    /// Structure des URLs CDN :
    ///   Sorts   : https://ddragon.leagueoflegends.com/cdn/{version}/img/spell/{filename}
    ///   Passifs : https://ddragon.leagueoflegends.com/cdn/{version}/img/passive/{filename}
    ///   Champions (miniature) :
    ///             https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/{filename}
    ///
    /// "latest" est accepté par le CDN Riot et retourne toujours les assets de
    /// la dernière version — utile comme fallback si l'API versions.json est injoignable.
    ///
    /// La locale des données (fr_FR, en_US…) provient de <see cref="AppLocalization.DataLocale"/>,
    /// elle-même déduite de la langue de l'application. Un seul endroit à changer.
    /// </summary>
    public static class DataDragonCdn
    {
        /// <summary>Alias Riot qui résout toujours vers la version la plus récente.</summary>
        public const string DefaultVersion = "latest";

        /// <summary>Racine du CDN DataDragon.</summary>
        public const string Base = "https://ddragon.leagueoflegends.com/cdn";

        /// <summary>Liste des versions de patch disponibles (JSON, le plus récent en premier).</summary>
        public const string VersionsUrl = "https://ddragon.leagueoflegends.com/api/versions.json";

        /// <summary>
        /// Version courante du patch DataDragon (ex : "14.11.1").
        /// Initialisée par <c>PatchVersionService</c> au démarrage.
        /// </summary>
        public static string Version { get; set; } = DefaultVersion;

        /// <summary>Locale des données Riot (fr_FR, en_US…), suit la langue de l'application.</summary>
        public static string DataLocale => AppLocalization.DataLocale;

        // ── URL builders ──────────────────────────────────────────────────

        /// <summary>
        /// URL de l'icône d'un sort actif.
        /// Ex : SpellUrl("AhriQ.png") → "https://…/cdn/14.11.1/img/spell/AhriQ.png"
        /// </summary>
        public static string SpellUrl(string filename)
            => $"{Base}/{Version}/img/spell/{filename}";

        /// <summary>
        /// URL de l'icône du passif d'un champion.
        /// Ex : PassiveUrl("AhriPassive.png") → "https://…/cdn/14.11.1/img/passive/AhriPassive.png"
        /// </summary>
        public static string PassiveUrl(string filename)
            => $"{Base}/{Version}/img/passive/{filename}";

        /// <summary>
        /// URL de la miniature du champion (carré 120×120 utilisé dans les listes).
        /// Ex : ChampionUrl("Ahri.png") → "https://…/cdn/14.11.1/img/champion/Ahri.png"
        /// </summary>
        public static string ChampionUrl(string filename)
            => $"{Base}/{Version}/img/champion/{filename}";

        /// <summary>
        /// URL du splash art d'un skin. À la différence des icônes, les splash
        /// arts ne sont PAS versionnés (pas de {version} dans le chemin).
        /// Ex : SkinUrl("Ahri_0.jpg") → "https://…/cdn/img/champion/splash/Ahri_0.jpg"
        /// </summary>
        public static string SkinUrl(string filename)
            => $"{Base}/img/champion/splash/{filename}";

        // ── API data ──────────────────────────────────────────────────────

        /// <summary>URL de la liste de tous les champions (langue <see cref="DataLocale"/>).</summary>
        public static string GeneralDataUrl()
            => $"{Base}/{Version}/data/{DataLocale}/champion.json";

        /// <summary>URL du détail d'un champion spécifique.</summary>
        public static string ChampionDetailUrl(string championName)
            => $"{Base}/{Version}/data/{DataLocale}/champion/{championName}.json";
    }
}
