namespace LOLInfo.Models;

using System.Collections.Generic;

/// <summary>
/// Catégories de ressources affichées dans le filtre, et mapping
/// vers les valeurs brutes de l'API Riot (Champion.Partype).
/// </summary>
public static class ChampionResources
{
    // ── Libellés des 4 catégories ────────────────────────────────────────

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
}
