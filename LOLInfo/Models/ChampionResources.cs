namespace LOLInfo.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Catégories de ressources affichées dans le filtre, et mapping
    /// vers les valeurs brutes de l'API Riot (Champion.Partype).
    ///
    /// Pour déplacer un partype dans une autre catégorie, modifiez
    /// uniquement les HashSet ci-dessous — aucun autre fichier à toucher.
    /// </summary>
    public static class ChampionResources
    {
        // ── Libellés des 4 catégories ────────────────────────────────────

        public const string Mana    = "Mana";
        public const string Aucun   = "Aucun";
        public const string Energie = "Énergie";
        public const string Autre   = "Autre";

        /// <summary>
        /// Ordre d'affichage dans la barre de filtres.
        /// </summary>
        public static readonly string[] CanonicalOrder =
        {
            Mana,
            Aucun,
            Energie,
            Autre,
        };

        // ── Mapping partype API → catégorie ──────────────────────────────

        /// <summary>Valeurs Riot qui correspondent à la ressource Mana.</summary>
        private static readonly HashSet<string> ManaValues = new()
        {
            "Mana",
        };

        /// <summary>Valeurs Riot qui correspondent à "aucune ressource".</summary>
        private static readonly HashSet<string> AucunValues = new()
        {
            "None",
        };

        /// <summary>Valeurs Riot qui correspondent à l'Énergie.</summary>
        private static readonly HashSet<string> EnergieValues = new()
        {
            "Energy",
        };

        // Tout ce qui n'est dans aucun des sets ci-dessus → Autre
        // (Rage, Courage, Shield, Blood Well, Crimson Rush, Flow, Heat…)

        // ── Résolution ───────────────────────────────────────────────────

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
}
