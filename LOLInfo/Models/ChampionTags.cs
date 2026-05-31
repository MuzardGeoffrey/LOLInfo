namespace LOLInfo.Models;

using LOLInfo.Properties;

/// <summary>
/// Constantes des tags Riot et leur ordre d'affichage canonique.
///
/// Les valeurs (Fighter, Tank…) sont les <b>clés brutes de l'API Riot</b> :
/// elles servent à la correspondance et ne doivent PAS être traduites.
/// L'affichage traduit est fourni par <see cref="GetLabel"/>.
/// </summary>
public static class ChampionTags
{
    public const string Fighter  = "Fighter";
    public const string Tank     = "Tank";
    public const string Mage     = "Mage";
    public const string Assassin = "Assassin";
    public const string Marksman = "Marksman";
    public const string Support  = "Support";

    /// <summary>Ordre d'affichage dans la barre de filtres.</summary>
    public static readonly string[] CanonicalOrder =
        [Fighter, Tank, Mage, Assassin, Marksman, Support];

    /// <summary>
    /// Libellé traduit (langue courante) pour un tag Riot.
    /// Tag inconnu → renvoyé tel quel (pas de traduction disponible).
    /// </summary>
    public static string GetLabel(string tag) => tag switch
    {
        Fighter  => Resources.Tag_Fighter,
        Tank     => Resources.Tag_Tank,
        Mage     => Resources.Tag_Mage,
        Assassin => Resources.Tag_Assassin,
        Marksman => Resources.Tag_Marksman,
        Support  => Resources.Tag_Support,
        _        => tag,
    };
}
