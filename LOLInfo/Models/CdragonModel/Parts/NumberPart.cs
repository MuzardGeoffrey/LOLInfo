namespace LOLInfo.Models.CdragonModel.Parts;

using System.Globalization;

/// <summary>
/// Constante numérique dans une formule CDragon.
/// Correspond à <c>{ "__type": "NumberCalculationPart", "mNumber": 0.45 }</c>.
/// </summary>
public class NumberPart(double value) : IFormulaPart
{
    /// <summary>Valeur constante de ce composant.</summary>
    public double Value { get; } = value;

    /// <inheritdoc />
    public string Format() =>
        this.Value == Math.Truncate(this.Value)
            ? ((int)this.Value).ToString()
            : this.Value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public double Evaluate(SpellContext context) => this.Value;
}
