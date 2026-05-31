namespace LOLInfo.Models;

/// <summary>
/// Clés d'identité des sorts d'un champion (touche d'activation).
///
/// Ce sont des identifiants techniques stables — pas du texte traduisible.
/// L'ordre de <see cref="Active"/> correspond à l'ordre des sorts renvoyé
/// par l'API Riot (Q, W, E, R).
/// </summary>
public static class SpellKeys
{
    public const string Passive = "Passif";
    public const string Q = "Q";
    public const string W = "W";
    public const string E = "E";
    public const string R = "R";

    /// <summary>Clés des sorts actifs, dans l'ordre de l'API Riot.</summary>
    public static readonly string[] Active = [Q, W, E, R];
}
