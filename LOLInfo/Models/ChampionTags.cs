namespace LOLInfo.Models;

/// <summary>
/// Constantes des tags Riot et leur ordre d'affichage canonique.
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
}
