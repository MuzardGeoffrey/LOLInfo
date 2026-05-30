namespace LOLInfo.Models.CdragonModel.Parts;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class NamedDataValuePart(string dataValueName, IReadOnlyDictionary<string, IReadOnlyList<double>>? dataValues) : IFormulaPart
{
    public string DataValueName { get; } = dataValueName ?? string.Empty;

    public IReadOnlyList<double> Values { get; } =
        dataValues is not null && dataValueName is not null && dataValues.TryGetValue(dataValueName, out var vals)
            ? vals
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
