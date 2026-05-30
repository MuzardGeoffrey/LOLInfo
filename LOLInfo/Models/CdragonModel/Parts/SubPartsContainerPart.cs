namespace LOLInfo.Models.CdragonModel.Parts;

using System.Collections.Generic;
using System.Linq;

public class ProductOfSubPartsPart(IReadOnlyList<IFormulaPart> subParts) : IFormulaPart
{
    public IReadOnlyList<IFormulaPart> SubParts { get; } = subParts;

    public string Format() =>
        this.SubParts.Count == 0 ? "?" :
        string.Join("×", this.SubParts.Select(p => $"({p.Format()})"));

    public double Evaluate(SpellContext context) => 0; // Phase 2
}

public class SumOfSubPartsPart(IReadOnlyList<IFormulaPart> subParts) : IFormulaPart
{
    public IReadOnlyList<IFormulaPart> SubParts { get; } = subParts;

    public string Format() =>
        this.SubParts.Count == 0 ? "?" :
        string.Join(" + ", this.SubParts.Select(p => p.Format()));

    public double Evaluate(SpellContext context) => 0; // Phase 2
}

public class StatBySubPartPart(ChampionStat stat, int formula, IFormulaPart subPart) : IFormulaPart
{
    public ChampionStat Stat    { get; } = stat;
    public int          Formula { get; } = formula;
    public IFormulaPart SubPart { get; } = subPart;

    public string Format()
    {
        var statLbl = this.Stat == (ChampionStat)(-1) ? "" : $" {this.Stat.ToLabel()}";
        return $"({this.SubPart.Format()}){statLbl}";
    }

    public double Evaluate(SpellContext context) => 0; // Phase 2
}

public class ClampSubPartsPart(IReadOnlyList<IFormulaPart> subParts) : IFormulaPart
{
    public IReadOnlyList<IFormulaPart> SubParts { get; } = subParts;

    public string Format() => this.SubParts.Count switch
    {
        >= 3 => $"clamp({this.SubParts[0].Format()}, {this.SubParts[1].Format()}, {this.SubParts[2].Format()})",
        2    => $"clamp({this.SubParts[0].Format()}, {this.SubParts[1].Format()})",
        1    => this.SubParts[0].Format(),
        _    => "?",
    };

    public double Evaluate(SpellContext context) => 0; // Phase 2
}
