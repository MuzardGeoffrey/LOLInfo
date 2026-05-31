namespace LOLInfo.Models.CdragonModel.Parts;

using System.Collections.Generic;
using System.Globalization;

using LOLInfo.Properties;

public class ByLevelFormulaPart(IReadOnlyList<double> values) : IFormulaPart
{
    public IReadOnlyList<double> Values { get; } = values;

    public string Format()
    {
        if (this.Values.Count == 0) return "?";

        var v1  = FormatVal(this.Values[0]);
        var v9  = this.Values.Count > 8  ? FormatVal(this.Values[8])  : null;
        var v18 = this.Values.Count > 17 ? FormatVal(this.Values[17]) : null;

        return (v9, v18) switch
        {
            (not null, not null) => $"{v1}/{v9}/{v18} {Resources.Formula_Level3}",
            (null, not null)     => $"{v1}/{v18} {Resources.Formula_Level2}",
            _                    => v1,
        };
    }

    private static string FormatVal(double v) =>
        v == (int)v ? ((int)v).ToString() : v.ToString("0.##", CultureInfo.InvariantCulture);

    public double Evaluate(SpellContext context) => 0; // Phase 2
}
