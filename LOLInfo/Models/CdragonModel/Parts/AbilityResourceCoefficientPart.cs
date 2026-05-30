namespace LOLInfo.Models.CdragonModel.Parts;

using System.Globalization;

/// <summary>
/// AbilityResourceByCoefficientCalculationPart — scaling sur la ressource du champion
/// (mana, énergie, rage…) multiplié par un coefficient.
/// Exemple : mCoefficient=0.02 → "+2% mana"
/// </summary>
public class AbilityResourceCoefficientPart(double coefficient) : IFormulaPart
{
    public double Coefficient { get; } = coefficient;

    public string Format()
    {
        var pct    = this.Coefficient * 100;
        var pctStr = pct == (int)pct ? $"{(int)pct}%" : $"{pct.ToString("0.##", CultureInfo.InvariantCulture)}%";
        return $"+{pctStr} ressource";
    }

    public double Evaluate(SpellContext context) => 0; // Phase 2
}
