namespace LOLInfo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows.Data;

    using LOLInfo.IViewModels;
    using LOLInfo.Models;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.Services;

    public class AllChampionViewModel : BaseViewModel, IAllChampionViewModel
    {
        private readonly IViewManager _viewManager;
        private readonly IRiotClient _httpRiot;

        private ObservableCollection<Champion> _champions = new();

        // ── Vue triée exposée à la View ──────────────────────────────────────
        private ICollectionView _championsView;

        public ICollectionView ChampionsView
        {
            get => _championsView;
            private set
            {
                _championsView = value;
                OnPropertyChanged(nameof(ChampionsView));
            }
        }

        // ── Tri ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Options de tri affichées dans le ComboBox.
        /// La clé est l'enum, la valeur est le libellé affiché.
        /// </summary>
        public List<KeyValuePair<SortOption, string>> SortOptions { get; } = new()
        {
            new(SortOption.NomAZ,        "Nom A → Z"),
            new(SortOption.NomZA,        "Nom Z → A"),
            new(SortOption.DifficulteAsc,  "Difficulté ↑"),
            new(SortOption.DifficulteDesc, "Difficulté ↓"),
        };

        private KeyValuePair<SortOption, string> _selectedSortOption;

        public KeyValuePair<SortOption, string> SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value;
                OnPropertyChanged(nameof(SelectedSortOption));
                ApplySort();
            }
        }

        // ── Constructeur ─────────────────────────────────────────────────────

        public AllChampionViewModel(IViewManager viewManager, IRiotClient httpRiot)
        {
            _viewManager = viewManager;
            _httpRiot = httpRiot;

            // Sélection par défaut : Nom A → Z (sans déclencher ApplySort, la vue n'existe pas encore)
            _selectedSortOption = SortOptions[0];
        }

        // ── Chargement des données ────────────────────────────────────────────

        public async Task GetAllChampions()
        {
            _champions = await _httpRiot.GetAllChampions();

            // CollectionViewSource.GetDefaultView retourne un ListCollectionView
            // pour une ObservableCollection, ce qui nous permet d'utiliser CustomSort.
            ChampionsView = CollectionViewSource.GetDefaultView(_champions);

            ApplySort();
        }

        // ── Tri ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Applique le tri sélectionné sur la CollectionView via CustomSort.
        /// On utilise CustomSort plutôt que SortDescriptions car ce dernier
        /// ne supporte pas les chemins de propriétés imbriquées (ex: Info.Difficulty).
        /// </summary>
        private void ApplySort()
        {
            if (ChampionsView is not ListCollectionView listView) return;

            listView.CustomSort = SelectedSortOption.Key switch
            {
                SortOption.NomAZ => Comparer<Champion>.Create(
                    (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)),

                SortOption.NomZA => Comparer<Champion>.Create(
                    (a, b) => string.Compare(b.Name, a.Name, StringComparison.OrdinalIgnoreCase)),

                SortOption.DifficulteAsc => Comparer<Champion>.Create(
                    (a, b) => (a.Info?.Difficulty ?? 0).CompareTo(b.Info?.Difficulty ?? 0)),

                SortOption.DifficulteDesc => Comparer<Champion>.Create(
                    (a, b) => (b.Info?.Difficulty ?? 0).CompareTo(a.Info?.Difficulty ?? 0)),

                _ => null
            };
        }
    }
}
