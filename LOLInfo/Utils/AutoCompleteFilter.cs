namespace LOLInfo.Utils;

using System;
using System.Collections.Generic;

/// <summary>
/// Cœur (pur, testable) de l'autocomplétion : sélectionne les éléments dont le
/// libellé contient la saisie, en priorisant les préfixes, sans doublon de libellé.
/// Utilisé par le contrôle <see cref="Views.AutoCompleteBox"/>.
/// </summary>
public static class AutoCompleteFilter
{
    /// <summary>
    /// Retourne au plus <paramref name="max"/> éléments dont le libellé (via
    /// <paramref name="display"/>) contient <paramref name="query"/> (insensible à la
    /// casse). Les correspondances par préfixe sont classées avant les autres, puis par
    /// ordre alphabétique. Les libellés en double sont fusionnés (première occurrence).
    /// Saisie vide ⇒ aucune suggestion.
    /// </summary>
    public static IReadOnlyList<T> Match<T>(IEnumerable<T>? items, string? query, Func<T, string> display, int max)
    {
        var result = new List<T>();
        if (items is null || max <= 0) return result;

        var q = (query ?? string.Empty).Trim();
        if (q.Length == 0) return result;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ranked = new List<(int Rank, string Label, T Item)>();

        foreach (var item in items)
        {
            var label = display(item) ?? string.Empty;
            if (label.Length == 0) continue;

            var idx = label.IndexOf(q, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;
            if (!seen.Add(label)) continue; // dédoublonnage par libellé

            ranked.Add((idx == 0 ? 0 : 1, label, item));
        }

        ranked.Sort((a, b) =>
        {
            var byRank = a.Rank.CompareTo(b.Rank);
            return byRank != 0 ? byRank : StringComparer.OrdinalIgnoreCase.Compare(a.Label, b.Label);
        });

        for (var i = 0; i < ranked.Count && i < max; i++)
            result.Add(ranked[i].Item);

        return result;
    }
}
