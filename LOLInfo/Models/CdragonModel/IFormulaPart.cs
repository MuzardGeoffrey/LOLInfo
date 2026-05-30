namespace LOLInfo.Models.CdragonModel;

public interface IFormulaPart
{
    string Format();
    double Evaluate(SpellContext context);
}
