namespace LOLInfo.ViewModels;

/// <summary>
/// Représente un item de filtre à cases à cocher (multi-sélection).
/// Utilisé pour les filtres Classes (Tags) et Ressources (Partype).
///
/// Sépare la <see cref="Key"/> (valeur stable de correspondance, ex : "Fighter")
/// du <see cref="Label"/> (texte affiché, traduit, ex : "Combattant").
/// Cette séparation permet la traduction de l'affichage sans casser le filtrage.
///
/// Pattern : AllChampionViewModel s'abonne à PropertyChanged de chaque instance.
/// Quand IsSelected change, AllChampionViewModel appelle ChampionsView.Refresh().
/// </summary>
public class FilterItemViewModel(string key, string label, bool isSelected = false) : BaseViewModel
{
    /// <summary>
    /// Crée un filtre dont la clé et le libellé sont identiques
    /// (utile quand aucune traduction n'est nécessaire).
    /// </summary>
    public FilterItemViewModel(string keyAndLabel, bool isSelected = false)
        : this(keyAndLabel, keyAndLabel, isSelected) { }

    /// <summary>
    /// Clé de correspondance stable, non traduite (ex : tag Riot "Fighter",
    /// catégorie "Énergie"). Utilisée par le filtrage. Immuable.
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// Texte affiché dans la CheckBox, traduit (ex : "Combattant", "Aucun").
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
