namespace LOLInfo.ViewModels
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Une ligne dans le tableau de progression par niveau d'un sort.
    ///
    /// Exemple : un sort dont le cooldown est "9/8/7/6/5" donne :
    ///   Label  = "⏱ Recharge"
    ///   Values = ["9", "8", "7", "6", "5"]
    ///
    /// Un sort avec une valeur constante, ex CooldownBurn = "7", donne :
    ///   Values = ["7"]  → affiché une seule fois dans la colonne
    ///
    /// Construction : new SpellStatRow("⏱ Recharge", spell.CooldownBurn)
    /// </summary>
    public class SpellStatRow
    {
        /// <summary>
        /// Libellé de la stat affichée à gauche du tableau.
        /// Ex : "⏱ Recharge", "💧 Coût", "🎯 Portée".
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Valeur à chaque niveau, une entrée par rang du sort.
        /// Obtenu en découpant le BurnString Riot par "/" :
        ///   "55/65/75/85/95" → ["55", "65", "75", "85", "95"]
        ///   "7"              → ["7"]
        /// </summary>
        public IReadOnlyList<string> Values { get; }

        /// <summary>
        /// Construit une ligne à partir du BurnString brut de l'API Riot.
        /// Si burnString est null ou vide, Values sera une liste vide
        /// et HasValues sera false.
        /// </summary>
        public SpellStatRow(string label, string? burnString)
        {
            Label = label;

            Values = string.IsNullOrWhiteSpace(burnString)
                ? Array.Empty<string>()
                : burnString.Split('/');
        }

        /// <summary>
        /// True si la ligne contient au moins une valeur à afficher.
        /// Permet de filtrer les rows vides avant affichage.
        /// </summary>
        public bool HasValues => Values.Count > 0;
    }
}
