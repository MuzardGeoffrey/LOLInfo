namespace LOLInfo.ViewModels;

/// <summary>
/// Données d'affichage d'un skin de champion (carrousel de l'onglet Général).
///
/// <see cref="SkinPath"/> est un simple nom de fichier (ex : "Ahri_0.jpg") ;
/// l'URL complète du splash art est construite par <c>ImagePathConverter</c>
/// via <c>ImageConstant.SKIN</c> — même pattern que <c>SpellViewModel.IconPath</c>.
/// Immuable : défini à la construction.
/// </summary>
public class SkinViewModel
{
    /// <summary>Identifiant Riot du champion (ex : "Ahri", "MonkeyKing").</summary>
    public required string ChampionId { get; init; }

    /// <summary>Numéro du skin (0 = skin de base).</summary>
    public int Num { get; init; }

    /// <summary>Nom affiché (le skin de base reprend le nom du champion).</summary>
    public required string DisplayName { get; init; }

    /// <summary>Nom de fichier du splash art (ex : "Ahri_0.jpg").</summary>
    public string SkinPath => $"{this.ChampionId}_{this.Num}.jpg";
}
