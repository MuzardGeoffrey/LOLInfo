namespace LOLInfo.Localization;

using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Point d'entrée UNIQUE de la gestion de la langue.
///
/// Pour changer la langue de l'application, il suffit de modifier
/// <see cref="DefaultCulture"/> ci-dessous (ex : "en" pour l'anglais).
/// Tout le reste — textes UI (.resx) et locale des données Riot (fr_FR/en_US) —
/// en découle automatiquement.
///
/// Les textes traduisibles vivent dans <c>Properties\Resources.resx</c> (français,
/// culture neutre) et ses satellites (<c>Resources.en.resx</c>, …).
/// Ajouter une langue = ajouter un <c>Resources.&lt;culture&gt;.resx</c> + une
/// entrée dans <see cref="DataLocaleByCulture"/> si la locale Riot diffère.
///
/// Le changement de langue à chaud (sans redémarrage) n'est pas géré pour
/// l'instant : la culture est appliquée une fois au démarrage.
/// </summary>
public static class AppLocalization
{
    /// <summary>
    /// Langue par défaut de l'application (code culture .NET : "fr", "en", …).
    /// SEUL endroit à modifier pour changer la langue.
    /// </summary>
    public const string DefaultCulture = "fr";

    /// <summary>Locale de données Riot utilisée quand la culture n'est pas mappée.</summary>
    public const string DefaultDataLocale = "fr_FR";

    /// <summary>
    /// Correspondance code culture .NET → locale DataDragon de Riot.
    /// Voir https://developer.riotgames.com (locales supportées par le CDN).
    /// </summary>
    private static readonly Dictionary<string, string> DataLocaleByCulture = new()
    {
        ["fr"] = "fr_FR",
        ["en"] = "en_US",
    };

    /// <summary>
    /// Applique la culture par défaut à l'ensemble de l'application.
    /// À appeler une seule fois, le plus tôt possible au démarrage,
    /// avant toute lecture de ressource ou tout appel réseau.
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
