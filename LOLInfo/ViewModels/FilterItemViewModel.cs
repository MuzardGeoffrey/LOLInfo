namespace LOLInfo.ViewModels
{
    /// <summary>
    /// Représente un item de filtre à cases à cocher (multi-sélection).
    /// Utilisé pour les filtres Classes (Tags) et Ressources (Partype).
    ///
    /// Pattern : AllChampionViewModel s'abonne à PropertyChanged de chaque instance.
    /// Quand IsSelected change, AllChampionViewModel appelle ChampionsView.Refresh().
    /// </summary>
    public class FilterItemViewModel : BaseViewModel
    {
        // ── Libellé affiché dans la View ─────────────────────────────────

        /// <summary>
        /// Texte affiché dans la CheckBox (ex : "Mage", "Fighter", "Mana").
        /// Immuable : défini à la construction, jamais modifié.
        /// </summary>
        public string Label { get; }

        // ── État coché / décoché ─────────────────────────────────────────

        private bool _isSelected;

        /// <summary>
        /// Quand ce champ passe à true ou false, PropertyChanged est levé.
        /// AllChampionViewModel a souscrit à cet événement pour appeler Refresh().
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return; // évite les notifications inutiles
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        // ── Constructeur ─────────────────────────────────────────────────

        /// <param name="label">Libellé de l'item.</param>
        /// <param name="isSelected">État initial (false par défaut = non sélectionné).</param>
        public FilterItemViewModel(string label, bool isSelected = false)
        {
            Label = label;
            _isSelected = isSelected;
        }
    }
}
