namespace LOLInfo.Models;

using System.Collections.Generic;

using LOLInfo.Properties;

/// <summary>
/// Catégories de ressources affichées dans le filtre, et mapping
/// vers les valeurs brutes de l'API Riot (Champion.Partype).
///
/// Les constantes Mana/Aucun/Énergie/Autre sont des <b>clés de catégorie</b>
/// stables (correspondance interne) ; l'affichage traduit est fourni par
/// <see cref="GetLabel"/>.
/// </summary>
public static class ChampionResources
{
    // ── Clés des 4 catégories ────────────────────────────────────────────

    public const string Mana    = "Mana";
    public const string Aucun   = "Aucun";
    public const string Energie = "Énergie";
    public const string Autre   = "Autre";

    /// <summary>Ordre d'affichage dans la barre de filtres.</summary>
    public static readonly string[] CanonicalOrder = [Mana, Aucun, Energie, Autre];

    // ── Mapping partype API → catégorie ──────────────────────────────────

    private static readonly HashSet<string> ManaValues    = ["Mana"];
    private static readonly HashSet<string> AucunValues   = ["Aucune"];
    private static readonly HashSet<string> EnergieValues = ["Énergie"];

    /// <summary>
    /// Retourne la catégorie d'affichage pour un partype brut de l'API.
    /// Si le partype est null, vide ou inconnu → "Autre".
    /// </summary>
    public static string GetCategory(string? partype)
    {
        if (string.IsNullOrEmpty(partype))   return Autre;
        if (ManaValues.Contains(partype))    return Mana;
        if (AucunValues.Contains(partype))   return Aucun;
        if (EnergieValues.Contains(partype)) return Energie;
        return Autre;
    }

    /// <summary>
    /// Libellé traduit (langue courante) pour une catégorie de ressource.
    /// Catégorie inconnue → renvoyée telle quelle.
    /// </summary>
    public static string GetLabel(string category) => category switch
    {
        Mana    => Resources.Resource_Mana,
        Aucun   => Resources.Resource_None,
        Energie => Resources.Resource_Energy,
        Autre   => Resources.Resource_Other,
        _       => category,
    };
}
