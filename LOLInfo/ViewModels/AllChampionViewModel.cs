namespace LOLInfo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
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

        // ── Filtre : Classes (Tags) ───────────────────────────────────────

        private List<FilterItemViewModel> _tagFilters = new();

        /// <summary>
        /// Construit après le chargement des champions.
        /// Chaque item représente un tag Riot (Fighter, Mage, etc.).
        /// La View bind une CheckBox sur chaque item.IsSelected.
        /// </summary>
        public IReadOnlyList<FilterItemViewModel> TagFilters => _tagFilters;

        // ── Filtre : Ressources (Partype) ────────────────────────────────

        private List<FilterItemViewModel> _partypeFilters = new();

        /// <summary>
        /// Construit après le chargement des champions.
        /// Chaque item représente un type de ressource (Mana, Energy, None…).
        /// </summary>
        public IReadOnlyList<FilterItemViewModel> PartypeFilters => _partypeFilters;

        // ── Filtre : Difficulté ───────────────────────────────────────────

        private int _difficultyMin = 0;

        /// <summary>
        /// Borne inférieure du filtre difficulté (0-10).
        /// Si DifficultyMin > valeur d'un champion, il est masqué.
        /// </summary>
        public int DifficultyMin
        {
            get => _difficultyMin;
            set
            {
                // On s'assure que Min ne dépasse pas Max
                var clamped = Math.Clamp(value, 0, _difficultyMax);
                if (_difficultyMin == clamped) return;
                _difficultyMin = clamped;
                OnPropertyChanged(nameof(DifficultyMin));
                _logger.LogDebug("Filtre difficulté min : {Value}", clamped);
                ChampionsView?.Refresh();
            }
        }

        private int _difficultyMax = 10;

        /// <summary>
        /// Borne supérieure du filtre difficulté (0-10).
        /// Si DifficultyMax < valeur d'un champion, il est masqué.
        /// </summary>
        public int DifficultyMax
        {
            get => _difficultyMax;
            set
            {
                // On s'assure que Max ne descend pas sous Min
                var clamped = Math.Clamp(value, _difficultyMin, 10);
                if (_difficultyMax == clamped) return;
                _difficultyMax = clamped;
                OnPropertyChanged(nameof(DifficultyMax));
                _logger.LogDebug("Filtre difficulté max : {Value}", clamped);
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

            _selectedSortOption = SortOptions[0];
            _logger.LogDebug("AllChampionViewModel initialisé — tri par défaut : {Sort}", _selectedSortOption.Value);
        }

        // ── Chargement ───────────────────────────────────────────────────

        public async Task GetAllChampions()
        {
            _logger.LogDebug("Début du chargement de la liste des champions");

            var champions = await _httpRiot.GetAllChampions();

            _items = new ObservableCollection<ChampionListItemViewModel>();
            foreach (var champion in champions)
                _items.Add(new ChampionListItemViewModel(champion, _favoritesService));

            // Construction des listes de filtres à partir des données réelles
            BuildTagFilters(champions);
            BuildPartypeFilters();

            ChampionsView = CollectionViewSource.GetDefaultView(_items);
            ChampionsView.Filter = ApplyFilter;

            ApplySort();

            _logger.LogInformation(
                "Vue initialisée — {Count} champion(s), {Tags} classes, {Partypes} ressources",
                _items.Count, _tagFilters.Count, _partypeFilters.Count);
        }

        // ── Construction des filtres multi-sélection ─────────────────────

        /// <summary>
        /// Extrait tous les tags distincts présents dans la liste chargée,
        /// dans l'ordre canonique Riot, puis s'abonne à leurs changements.
        /// </summary>
        private void BuildTagFilters(IEnumerable<Champion> champions)
        {
            // Ordre canonique défini dans ChampionTags.CanonicalOrder (Models/ChampionTags.cs).
            // Modifiez ce fichier pour changer l'ordre ou ajouter des tags.
            var canonicalOrder = ChampionTags.CanonicalOrder;

            // On extrait les tags présents dans les données (au cas où Riot en ajouterait)
            var presentTags = champions
                .SelectMany(c => c.Tags ?? Enumerable.Empty<string>())
                .Distinct()
                .ToHashSet();

            // On construit dans l'ordre canonique, puis on ajoute les éventuels inconnus
            var ordered = canonicalOrder
                .Where(t => presentTags.Contains(t))
                .Concat(presentTags.Except(canonicalOrder).OrderBy(t => t));

            _tagFilters = ordered
                .Select(tag => new FilterItemViewModel(tag))
                .ToList();

            SubscribeToFilterItems(_tagFilters);
            OnPropertyChanged(nameof(TagFilters));

            _logger.LogDebug("TagFilters construits : {Tags}", string.Join(", ", _tagFilters.Select(f => f.Label)));
        }

        /// <summary>
        /// Construit les 4 catégories de ressources définies dans ChampionResources.cs.
        /// L'ordre et les libellés sont centralisés dans ChampionResources.CanonicalOrder.
        /// Le mapping partype API → catégorie est géré par ChampionResources.GetCategory().
        /// </summary>
        private void BuildPartypeFilters()
        {
            _partypeFilters = ChampionResources.CanonicalOrder
                .Select(category => new FilterItemViewModel(category))
                .ToList();

            SubscribeToFilterItems(_partypeFilters);
            OnPropertyChanged(nameof(PartypeFilters));

            _logger.LogDebug("PartypeFilters construits : {Partypes}", string.Join(", ", _partypeFilters.Select(f => f.Label)));
        }

        /// <summary>
        /// S'abonne au PropertyChanged de chaque FilterItemViewModel.
        /// Quand IsSelected change sur n'importe quel item,
        /// on appelle Refresh() pour mettre à jour la vue.
        ///
        /// C'est le pattern Observer : le parent (ce ViewModel) observe
        /// ses enfants (les FilterItemViewModel) pour réagir à leurs changements.
        /// </summary>
        private void SubscribeToFilterItems(IEnumerable<FilterItemViewModel> items)
        {
            foreach (var item in items)
            {
                item.PropertyChanged += OnFilterItemChanged;
            }
        }

        private void OnFilterItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterItemViewModel.IsSelected))
            {
                _logger.LogDebug("Filtre item modifié : '{Label}' = {IsSelected}",
                    (sender as FilterItemViewModel)?.Label,
                    (sender as FilterItemViewModel)?.IsSelected);

                ChampionsView?.Refresh();
            }
        }

        // ── Tri ──────────────────────────────────────────────────────────

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

        // ── Filtre central ────────────────────────────────────────────────

        /// <summary>
        /// Prédicat central appelé pour chaque item à chaque Refresh().
        /// Retourne true = visible, false = masqué.
        ///
        /// Combinaison AND entre les groupes de filtres :
        ///   un champion doit passer TOUS les filtres actifs.
        ///
        /// Au sein des multi-sélections (tags, partype) : logique OR
        ///   → le champion est visible s'il correspond À AU MOINS UNE des cases cochées.
        ///   → si aucune case n'est cochée, le filtre est inactif (tout visible).
        ///
        /// Ordre : du test le plus rapide (bool) au plus coûteux (LINQ/string).
        /// </summary>
        private bool ApplyFilter(object obj)
        {
            if (obj is not ChampionListItemViewModel item) return false;

            // 1. Favori
            if (ShowFavoritesOnly && !item.IsFavorite)
                return false;

            // 2. Type de dégâts
            if (SelectedDamageType != DamageTypeFilter.Tous && item.DamageType != SelectedDamageType)
                return false;

            // 3. Portée
            if (SelectedRangeType == RangeTypeFilter.Melee &&  item.IsRanged) return false;
            if (SelectedRangeType == RangeTypeFilter.Range && !item.IsRanged) return false;

            // 4. Difficulté — filtre inactif si les bornes sont au maximum (0-10)
            var difficulty = item.Champion.Info?.Difficulty ?? 0;
            if (difficulty < DifficultyMin || difficulty > DifficultyMax)
                return false;

            // 5. Classes (Tags) — logique OR, inactif si aucune case cochée
            var selectedTags = _tagFilters.Where(f => f.IsSelected).ToList();
            if (selectedTags.Count > 0)
            {
                var championTags = item.Champion.Tags ?? new List<string>();
                // Le champion doit avoir AU MOINS UN tag parmi ceux sélectionnés
                if (!selectedTags.Any(f => championTags.Contains(f.Label)))
                    return false;
            }

            // 6. Ressources — logique OR, inactif si aucune case cochée.
            //    ChampionResources.GetCategory() traduit le partype brut de l'API
            //    ("None", "Energy"…) en catégorie d'affichage ("Aucun", "Énergie"…).
            var selectedPartypes = _partypeFilters.Where(f => f.IsSelected).ToList();
            if (selectedPartypes.Count > 0)
            {
                var category = ChampionResources.GetCategory(item.Champion.Partype);
                if (!selectedPartypes.Any(f => f.Label == category))
                    return false;
            }

            // 7. Nom
            if (!string.IsNullOrEmpty(NameFilter) &&
                !(item.Champion.Name?.Contains(NameFilter, StringComparison.OrdinalIgnoreCase) ?? false))
                return false;

            return true;
        }
    }
}
