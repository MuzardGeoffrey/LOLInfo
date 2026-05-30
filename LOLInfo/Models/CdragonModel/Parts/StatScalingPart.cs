namespace LOLInfo.Models.CdragonModel.Parts;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class StatScalingPart(
    int mStat,
    int mStatFormula,
    string dataValueName,
    IReadOnlyDictionary<string, IReadOnlyList<double>>? dataValues) : IFormulaPart
{
    public ChampionStat Stat          { get; } = (ChampionStat)mStat;
    public StatFormula  Formula       { get; } = (StatFormula)mStatFormula;
    public string       DataValueName { get; } = dataValueName ?? string.Empty;

    public IReadOnlyList<double> Ratios { get; } =
        dataValues is not null && dataValueName is not null && dataValues.TryGetValue(dataValueName, out var vals)
            ? vals
            : [];

    public string Format()
    {
        if (this.Ratios.Count == 0) return $"+? {this.Stat.ToLabel(this.Formula)}";

        bool isPercent = this.Stat is
            ChampionStat.AbilityPower or ChampionStat.AttackDamage or
            ChampionStat.Armor        or ChampionStat.MagicResist   or
            ChampionStat.Health       or ChampionStat.Mana;

        string ratioStr;
        if (isPercent)
        {
            ratioStr = string.Join("/", this.Ratios.Select(r =>
            {
                double pct = r * 100.0;
                return pct == Math.Truncate(pct) ? ((int)pct).ToString() : pct.ToString("0.#", CultureInfo.InvariantCulture);
            })) + "%";
        }
        else
        {
            ratioStr = string.Join("/", this.Ratios.Select(r =>
                r == Math.Truncate(r) ? ((int)r).ToString() : r.ToString("0.##", CultureInfo.InvariantCulture)));
        }

        return $"+{ratioStr} {this.Stat.ToLabel(this.Formula)}";
    }

    public double Evaluate(SpellContext context) =>
        throw new NotImplementedException("Phase 2 — calculateur non encore implémenté.");
}
