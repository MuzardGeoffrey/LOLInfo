namespace LOLInfo.Models.CdragonModel.Parts;

using System.Globalization;

using LOLInfo.Properties;

public class ByLevelInterpolationPart(double startValue, double endValue) : IFormulaPart
{
    public double StartValue { get; } = startValue;
    public double EndValue   { get; } = endValue;

    public string Format() =>
        $"{FormatVal(this.StartValue)}–{FormatVal(this.EndValue)} {Resources.Formula_LevelRange}";

    private static string FormatVal(double v) =>
        v == (int)v ? ((int)v).ToString() : v.ToString("0.##", CultureInfo.InvariantCulture);

    public double Evaluate(SpellContext context) => 0; // Phase 2
}
