namespace LOLInfo.Localization;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Point d'entrée UNIQUE de la gestion de la langue.
///
/// La langue effective est persistée par <c>LanguageService</c> et rechargée à
/// chaque démarrage. <see cref="DefaultCulture"/> n'est utilisée qu'au tout
/// premier lancement (aucune préférence enregistrée).
///
/// Les textes traduisibles vivent dans <c>Properties\Resources.resx</c> (français,
/// culture neutre) et ses satellites (<c>Resources.en.resx</c>, …).
/// Ajouter une langue = ajouter un <c>Resources.&lt;culture&gt;.resx</c> + une
/// entrée dans <see cref="DataLocaleByCulture"/> si la locale Riot diffère.
/// </summary>
public static class AppLocalization
{
    /// <summary>
    /// Langue par défaut au tout premier lancement (code culture .NET : "en", "fr"…).
    /// Une fois une langue choisie, elle est persistée et a la priorité.
    /// </summary>
    public const string DefaultCulture = "en";

    /// <summary>Locale de données Riot utilisée quand la culture n'est pas mappée.</summary>
    public const string DefaultDataLocale = "en_US";

    /// <summary>
    /// Correspondance code culture .NET → locale DataDragon de Riot.
    /// Voir https://developer.riotgames.com (locales supportées par le CDN).
    /// </summary>
    private static readonly Dictionary<string, string> DataLocaleByCulture = new()
    {
        ["en"] = "en_US",
        ["fr"] = "fr_FR",
    };

    /// <summary>Langues proposées par l'application (codes culture .NET).</summary>
    public static IReadOnlyList<string> SupportedCultures { get; } =
        DataLocaleByCulture.Keys.ToArray();

    /// <summary>True si <paramref name="cultureName"/> fait partie des langues supportées.</summary>
    public static bool IsSupported(string? cultureName) =>
        cultureName is not null && DataLocaleByCulture.ContainsKey(cultureName);

    /// <summary>
    /// Applique la culture par défaut à l'ensemble de l'application.
    /// </summary>
    public static void ApplyDefaultCulture() => ApplyCulture(DefaultCulture);

    /// <summary>
    /// Applique la culture indiquée au thread courant et aux threads futurs.
    /// </summary>
    public static void ApplyCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);

        CultureInfo.CurrentCulture   = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture   = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    /// <summary>
    /// Locale de données Riot (ex : "fr_FR") correspondant à la culture UI courante.
    /// Lue par <c>DataDragonCdn</c> pour construire les URLs de données.
    /// </summary>
    public static string DataLocale
    {
        get
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return DataLocaleByCulture.TryGetValue(lang, out var locale)
                ? locale
                : DefaultDataLocale;
        }
    }
}
