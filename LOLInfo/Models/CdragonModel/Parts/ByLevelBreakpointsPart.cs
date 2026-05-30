namespace LOLInfo.Models.CdragonModel.Parts;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class ByLevelBreakpointsPart(double level1Value, IReadOnlyList<(int Level, double Bonus)> breakpoints) : IFormulaPart
{
    public double Level1Value { get; } = level1Value;
    public IReadOnlyList<(int Level, double Bonus)> Breakpoints { get; } = breakpoints;

    public string Format()
    {
        if (!this.Breakpoints.Any()) return FormatVal(this.Level1Value);

        var cumul = this.Level1Value;
        List<double> values = [this.Level1Value];
        foreach (var (_, bonus) in this.Breakpoints)
        {
            cumul += bonus;
            values.Add(cumul);
        }

        return string.Join("→", values.Select(FormatVal)) + " (par niv.)";
    }

    private static string FormatVal(double v) =>
        v == (int)v ? ((int)v).ToString() : v.ToString("0.##", CultureInfo.InvariantCulture);

    public double Evaluate(SpellContext context) => 0; // Phase 2
}
