namespace LOLInfo.Models.CdragonModel.Parts;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class BuffCounterDataValuePart(IReadOnlyList<double> valuesPerStack) : IFormulaPart
{
    public IReadOnlyList<double> ValuesPerStack { get; } = valuesPerStack;

    public string Format()
    {
        if (this.ValuesPerStack.Count == 0) return "[stacks]×?";
        return $"[stacks]×{string.Join("/", this.ValuesPerStack.Select(FormatVal))}";
    }

    private static string FormatVal(double v) =>
        v == (int)v ? ((int)v).ToString() : v.ToString("0.##", CultureInfo.InvariantCulture);

    public double Evaluate(SpellContext context) => 0; // Phase 2
}

public class BuffCounterCoefficientPart(double coefficient) : IFormulaPart
{
    public double Coefficient { get; } = coefficient;

    public string Format()
    {
        var c = this.Coefficient == (int)this.Coefficient
            ? ((int)this.Coefficient).ToString()
            : this.Coefficient.ToString("0.##", CultureInfo.InvariantCulture);
        return $"[stacks]×{c}";
    }

    public double Evaluate(SpellContext context) => 0; // Phase 2
}
