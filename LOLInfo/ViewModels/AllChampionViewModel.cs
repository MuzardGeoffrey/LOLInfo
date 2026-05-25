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
    using LOLInfo.Services.Storage;

    using Microsoft.Extensions.Logging;

    public class AllChampionViewModel : BaseViewModel, IAllChampionViewModel
    {
        private readonly IViewManager _viewManager;
        private readonly IRiotClient _httpRiot;
        private readonly IFavoritesService _favoritesService;
        private readonly ILogger<AllChampionViewModel> _logger;

        private ObservableCollection<ChampionListItemViewModel> _items = new();

        // ── Vue triée + filtrée exposée à la View ────────────────────────
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

        // ── Tri ──────────────────────────────────────────────────────────

        public List<KeyValuePair<SortOption, string>> SortOptions { get; } = new()
        {
            new(SortOption.NomAZ,          "Nom A → Z"),
            new(SortOption.NomZA,          "Nom Z → A"),
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

        // ── Filtre : Favoris ─────────────────────────────────────────────

        private bool _showFavoritesOnly;

        public bool ShowFavoritesOnly
        {
            get => _showFavoritesOnly;
            set
            {
                _showFavoritesOnly = value;
                OnPropertyChanged(nameof(ShowFavoritesOnly));
                _logger.LogDebug("Filtre favoris : {Value}", value ? "activé" : "désactivé");
                ChampionsView?.Refresh();
            }
        }

        // ── Filtre : Nom ─────────────────────────────────────────────────

        private string _nameFilter = string.Empty;

        /// <summary>
        /// Texte tapé dans la barre de recherche.
        /// Le setter déclenche un Refresh() de la CollectionView
        /// pour que l'affichage se mette à jour instantanément.
        /// </summary>
        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                _nameFilter = value ?? string.Empty;
                OnPropertyChanged(nameof(NameFilter));
                _logger.LogDebug("Filtre nom : '{NameFilter}'", _nameFilter);
                ChampionsView?.Refresh();
            }
        }

        // ── Filtre : Type de dégâts ──────────────────────────────────────

        private DamageTypeFilter _selectedDamageType = DamageTypeFilter.Tous;

        public DamageTypeFilter SelectedDamageType
        {
            get => _selectedDamageType;
            set
            {
                _selectedDamageType = value;
                OnPropertyChanged(nameof(SelectedDamageType));
                _logger.LogDebug("Filtre dégâts : {Value}", value);
                ChampionsView?.Refresh();
            }
        }

        // ── Filtre : Portée ──────────────────────────────────────────────

        private RangeTypeFilter _selectedRangeType = RangeTypeFilter.Tous;

        public RangeTypeFilter SelectedRangeType
        {
            get => _selectedRangeType;
            set
            {
                _selectedRangeType = value;
                OnPropertyChanged(nameof(SelectedRangeType));
                _logger.LogDebug("Filtre portée : {Value}", value);
                ChampionsView?.Refresh();
            }
        }

        // ── Constructeur ──────────────────────────────────────────────────

        public AllChampionViewModel(IViewManager viewManager, IRiotClient httpRiot, IFavoritesService favoritesService, ILogger<AllChampionViewModel> logger)
        {
            _viewManager = viewManager;
            _httpRiot = httpRiot;
            _favoritesService = favoritesService;
            _logger = logger;

            _selectedSortOption = SortOptions[0]; // Nom A → Z par défaut
            _logger.LogDebug("AllChampionViewModel initialisé — tri par défaut : {Sort}", _selectedSortOption.Value);
        }

        // ── Chargement ───────────────────────────────────────────────────

        public async Task GetAllChampions()
        {
            _logger.LogDebug("Début du chargement de la liste des champions");

            var champions = await _httpRiot.GetAllChampions();

            // Chaque Champion est wrappé dans un ChampionListItemViewModel
            // qui porte son propre IsFavorite, ToggleFavoriteCommand,
            // DamageType (calculé) et IsRanged (calculé).
            _items = new ObservableCollection<ChampionListItemViewModel>();
            foreach (var champion in champions)
                _items.Add(new ChampionListItemViewModel(champion, _favoritesService));

            // CollectionViewSource.GetDefaultView() retourne un ICollectionView
            // (en pratique un ListCollectionView pour les ObservableCollection).
            // On y branche notre prédicat de filtre : ApplyFilter sera appelé
            // pour chaque item à chaque Refresh().
            ChampionsView = CollectionViewSource.GetDefaultView(_items);
            ChampionsView.Filter = ApplyFilter;

            ApplySort();

            _logger.LogInformation("Vue initialisée — {Count} champion(s) chargé(s), tri : {Sort}",
                _items.Count, _selectedSortOption.Value);
        }

        // ── Tri ──────────────────────────────────────────────────────────

        /// <summary>
        /// Utilise ListCollectionView.CustomSort pour supporter les propriétés
        /// imbriquées (Champion.Info.Difficulty) sans passer par SortDescriptions.
        /// </summary>
        private void ApplySort()
        {
            if (ChampionsView is not ListCollectionView listView) return;

            _logger.LogDebug("Application du tri : {Sort}", SelectedSortOption.Value);

            listView.CustomSort = SelectedSortOption.Key switch
            {
                SortOption.NomAZ => Comparer<ChampionListItemViewModel>.Create(
                    (a, b) => string.Compare(a.Champion.Name, b.Champion.Name, StringComparison.OrdinalIgnoreCase)),

                SortOption.NomZA => Comparer<ChampionListItemViewModel>.Create(
                    (a, b) => string.Compare(b.Champion.Name, a.Champion.Name, StringComparison.OrdinalIgnoreCase)),

                SortOption.DifficulteAsc => Comparer<ChampionListItemViewModel>.Create(
                    (a, b) => (a.Champion.Info?.Difficulty ?? 0).CompareTo(b.Champion.Info?.Difficulty ?? 0)),

                SortOption.DifficulteDesc => Comparer<ChampionListItemViewModel>.Create(
                    (a, b) => (b.Champion.Info?.Difficulty ?? 0).CompareTo(a.Champion.Info?.Difficulty ?? 0)),

                _ => null
            };
        }

        // ── Filtre ───────────────────────────────────────────────────────

        /// <summary>
        /// Prédicat central : appelé par la CollectionView pour chaque item
        /// à chaque Refresh(). Retourne true = visible, false = caché.
        ///
        /// Tous les filtres sont combinés en AND :
        /// un champion doit passer TOUS les filtres actifs pour être affiché.
        ///
        /// Ordre des conditions : du plus rapide au plus coûteux,
        /// pour court-circuiter le plus tôt possible via l'opérateur &&.
        /// </summary>
        private bool ApplyFilter(object obj)
        {
            if (obj is not ChampionListItemViewModel item) return false;

            // 1. Filtre favori — lecture directe d'un bool
            if (ShowFavoritesOnly && !item.IsFavorite)
                return false;

            // 2. Filtre type de dégâts — comparaison d'enum (valeur calculée à la construction)
            if (SelectedDamageType != DamageTypeFilter.Tous && item.DamageType != SelectedDamageType)
                return false;

            // 3. Filtre portée — comparaison bool (valeur calculée à la construction)
            if (SelectedRangeType == RangeTypeFilter.Melee  &&  item.IsRanged) return false;
            if (SelectedRangeType == RangeTypeFilter.Range  && !item.IsRanged) return false;

            // 4. Filtre nom — comparaison de chaîne, insensible à la casse
            //    Placé en dernier car Contains est légèrement plus coûteux.
            if (!string.IsNullOrEmpty(NameFilter) &&
                !(item.Champion.Name?.Contains(NameFilter, StringComparison.OrdinalIgnoreCase) ?? false))
                return false;

            return true;
        }
    }
}
