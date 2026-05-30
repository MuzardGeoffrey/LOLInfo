namespace LOLInfo.ViewModels;

/// <summary>
/// Représente une ligne du tableau "gain par niveau" d'un sort.
/// Zippe les deux listes parallèles Label / Effect du leveltip Riot.
/// </summary>
public class LeveltipRowViewModel(string label, string effect)
{
    /// <summary>Libellé de la statistique (ex : "Dégâts", "Temps de recharge").</summary>
    public string Label  { get; } = label;

    /// <summary>Valeurs aux niveaux 1-5 séparées par "/" (ex : "60/95/130/165/200").</summary>
    public string Effect { get; } = effect;
}
