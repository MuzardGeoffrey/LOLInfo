namespace LOLInfo.Models.CdragonModel.Parts;

using System.Globalization;

public class StatByCoefficientPart(double coefficient, int statId, int statFormula) : IFormulaPart
{
    public double      Coefficient { get; } = coefficient;
    public ChampionStat Stat       { get; } = Enum.IsDefined((ChampionStat)statId) ? (ChampionStat)statId : ChampionStat.Unknown;
    public StatFormula  Formula    { get; } = Enum.IsDefined((StatFormula)statFormula) ? (StatFormula)statFormula : StatFormula.Total;

    public string Format()
    {
        var pct    = this.Coefficient * 100;
        var pctStr = pct == (int)pct ? $"{(int)pct}%" : $"{pct.ToString("0.##", CultureInfo.InvariantCulture)}%";
        // Pas de '+' interne : la jointure (SpellCalculation.Format) l'ajoute déjà.
        return this.Stat == ChampionStat.Unknown
            ? pctStr
            : $"{pctStr} {this.Stat.ToLabel(this.Formula)}";
    }

    public double Evaluate(SpellContext context) => 0; // Phase 2
}
