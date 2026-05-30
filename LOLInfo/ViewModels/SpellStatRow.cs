namespace LOLInfo.ViewModels;

using System.Collections.Generic;

/// <summary>
/// Une ligne dans le tableau de progression par niveau d'un sort.
/// Ex : Label = "⏱ Recharge", Values = ["9","8","7","6","5"]
/// </summary>
public class SpellStatRow(string label, string? burnString)
{
    /// <summary>Libellé affiché à gauche (ex : "⏱ Recharge", "💧 Coût", "🎯 Portée").</summary>
    public string Label { get; } = label;

    /// <summary>
    /// Valeurs par rang, obtenues en découpant le BurnString Riot par "/" :
    /// "55/65/75/85/95" → ["55","65","75","85","95"]
    /// </summary>
    public IReadOnlyList<string> Values { get; } =
        string.IsNullOrWhiteSpace(burnString) ? [] : burnString.Split('/');

    /// <summary>True si la ligne contient au moins une valeur.</summary>
    public bool HasValues => Values.Count > 0;
}
