namespace LOLInfo.Models.CdragonModel;

using System.Collections.Generic;
using System.Linq;

public class SpellCalculation(string name, IReadOnlyList<IFormulaPart>? parts = null)
{
    public string Name { get; } = name ?? string.Empty;
    public IReadOnlyList<IFormulaPart> Parts { get; } = parts ?? [];

    public string Format()
    {
        if (this.Parts.Count == 0) return string.Empty;
        return string.Join(" + ", this.Parts
            .Select(p => p.Format())
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
