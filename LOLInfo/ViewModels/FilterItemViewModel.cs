namespace LOLInfo.ViewModels;

/// <summary>
/// Représente un item de filtre à cases à cocher (multi-sélection).
/// Utilisé pour les filtres Classes (Tags) et Ressources (Partype).
///
/// Pattern : AllChampionViewModel s'abonne à PropertyChanged de chaque instance.
/// Quand IsSelected change, AllChampionViewModel appelle ChampionsView.Refresh().
/// </summary>
public class FilterItemViewModel(string label, bool isSelected = false) : BaseViewModel
{
    /// <summary>
    /// Texte affiché dans la CheckBox (ex : "Mage", "Fighter", "Mana").
    /// Immuable : défini à la construction.
    /// </summary>
    public string Label { get; } = label;

    /// <summary>
    /// Quand ce champ change, PropertyChanged est levé.
    /// AllChampionViewModel a souscrit pour appeler Refresh().
    /// </summary>
    public bool IsSelected
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            this.OnPropertyChanged(nameof(IsSelected));
        }
    } = isSelected;
}
