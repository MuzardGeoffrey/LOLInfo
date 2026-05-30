namespace LOLInfo.Models.CdragonModel.Parts;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Valeur indexée dans le tableau <c>mEffectAmount</c> du sort CDragon.
/// <c>mEffectIndex</c> indexe directement <c>mEffectAmount</c> : l'index 0 est un
/// emplacement réservé, l'effet 1 correspond donc à <c>mEffectAmount[1]</c>.
/// </summary>
public class EffectValuePart(int effectIndex, IReadOnlyList<IReadOnlyList<double>>? effectAmount) : IFormulaPart
{
    public int EffectIndex { get; } = effectIndex;

    public IReadOnlyList<double> Values { get; } =
        effectAmount is not null && effectIndex >= 0 && effectIndex < effectAmount.Count
            ? effectAmount[effectIndex]
            : [];

    public string Format()
    {
        if (this.Values.Count == 0) return "?";
        return string.Join("/", this.Values.Select(v =>
            v == Math.Truncate(v) ? ((int)v).ToString() : v.ToString("0.##", CultureInfo.InvariantCulture)));
    }

    public double Evaluate(SpellContext context) =>
        throw new NotImplementedException("Phase 2 — calculateur non encore implémenté.");
}
