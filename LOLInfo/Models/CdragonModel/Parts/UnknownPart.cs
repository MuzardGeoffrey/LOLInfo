namespace LOLInfo.Models.CdragonModel.Parts;

public class UnknownPart(string? typeName) : IFormulaPart
{
    public string TypeName { get; } = typeName ?? "null";

    public string Format() => $"[{this.TypeName}?]";

    public double Evaluate(SpellContext context) =>
        throw new NotImplementedException($"Type CDragon inconnu '{this.TypeName}' — calculateur non implémenté.");
}
