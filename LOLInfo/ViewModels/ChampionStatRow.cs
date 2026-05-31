namespace LOLInfo.ViewModels;

/// <summary>
/// Une ligne de la page Stats : libellé traduit + valeur formatée pour le niveau choisi.
/// </summary>
public class ChampionStatRow(string label, string value)
{
    public string Label { get; } = label;
    public string Value { get; } = value;
}
