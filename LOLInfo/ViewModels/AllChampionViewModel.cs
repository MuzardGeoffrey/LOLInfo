namespace LOLInfo.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

using LOLInfo.IServices;
using LOLInfo.IServices.Storage;
using LOLInfo.IViewModels;
using LOLInfo.Models;
using LOLInfo.Models.RiotModel;

using Microsoft.Extensions.Logging;

public class AllChampionViewModel(
    IRiotClient httpRiot,
    IFavoritesService favoritesService,
    ILogger<AllChampionViewModel> logger) : BaseViewModel, IAllChampionViewModel
{
    private ObservableCollection<ChampionListItemViewModel> _items = [];

    // ── Vue triée + filtrée ───────────────────────────────────────────────

    private ICollectionView _championsView;

    public ICollectionView ChampionsView
    {
        get => this._championsView;
        private set { this._championsView = value; this.OnPropertyChanged(nameof(ChampionsView)); }
    }

    // ── Tri ───────────────────────────────────────────────────────────────

    public List<KeyValuePair<SortOption, string>> SortOptions { get; } =
    [
        new(SortOption.NomAZ,          "Nom A → Z"),
        new(SortOption.NomZA,          "Nom Z → A"),
        new(SortOption.DifficulteAsc,  "Difficulté ↑"),
        new(SortOption.DifficulteDesc, "Difficulté ↓"),
    ];

    private KeyValuePair<SortOption, string> _selectedSortOption = new(SortOption.NomAZ, "Nom A → Z");

    public KeyValuePair<SortOption, string> SelectedSortOption
    {
        get => this._selectedSortOption;
        set { this._selectedSortOption = value; this.OnPropertyChanged(nameof(SelectedSortOption)); this.ApplySort(); }
    }

    // ── Filtre : Favoris ──────────────────────────────────────────────────

    public bool ShowFavoritesOnly
    {
        get;
        set
        {
            field = value;
            this.OnPropertyChanged(nameof(ShowFavoritesOnly));
            logger.LogDebug("Filtre favoris : {Value}", value ? "activé" : "désactivé");
            this.ChampionsView?.Refresh();
        }
    }

    // ── Filtre : Nom ──────────────────────────────────────────────────────

    public string NameFilter
    {
        get;
        set
        {
            field = value ?? string.Empty;
            this.OnPropertyChanged(nameof(NameFilter));
            logger.LogDebug("Filtre nom : '{NameFilter}'", field);
            this.ChampionsView?.Refresh();
        }
    } = string.Empty;

    // ── Filtre : Type de dégâts ───────────────────────────────────────────

    public DamageTypeFilter SelectedDamageType
    {
        get;
        set
        {
            field = value;
            this.OnPropertyChanged(nameof(SelectedDamageType));
            logger.LogDebug("Filtre dégâts : {Value}", value);
            this.ChampionsView?.Refresh();
        }
    } = DamageTypeFilter.Tous;

    // ── Filtre : Portée ───────────────────────────────────────────────────

    public RangeTypeFilter SelectedRangeType
    {
        get;
        set
        {
            field = value;
            this.OnPropertyChanged(nameof(SelectedRangeType));
            logger.LogDebug("Filtre portée : {Value}", value);
            this.ChampionsView?.Refresh();
        }
    } = RangeTypeFilter.Tous;

    // ── Filtre : Classes (Tags) ───────────────────────────────────────────

    private List<FilterItemViewModel> _tagFilters = [];
    public IReadOnlyList<FilterItemViewModel> TagFilters => this._tagFilters;

    // ── Filtre : Ressources (Partype) ─────────────────────────────────────

    private List<FilterItemViewModel> _partypeFilters = [];
    public IReadOnlyList<FilterItemViewModel> PartypeFilters => this._partypeFilters;

    // ── Filtre : Difficulté ───────────────────────────────────────────────

    private int _difficultyMax = 10;

    public int DifficultyMin
    {
        get;
        set
        {
            var clamped = Math.Clamp(value, 0, this._difficultyMax);
            if (field == clamped) return;
            field = clamped;
            this.OnPropertyChanged(nameof(DifficultyMin));
            logger.LogDebug("Filtre difficulté min : {Value}", clamped);
            this.ChampionsView?.Refresh();
        }
    }

    public int DifficultyMax
    {
        get => this._difficultyMax;
        set
        {
            var clamped = Math.Clamp(value, this.DifficultyMin, 10);
            if (this._difficultyMax == clamped) return;
            this._difficultyMax = clamped;
            this.OnPropertyChanged(nameof(DifficultyMax));
            logger.LogDebug("Filtre difficulté max : {Value}", clamped);
            this.ChampionsView?.Refresh();
        }
    }

    // ── Chargement ────────────────────────────────────────────────────────

    public async Task GetAllChampions()
    {
        logger.LogDebug("Début du chargement de la liste des champions");

        var champions = await httpRiot.GetAllChampions();

        this._items = [];
        foreach (var champion in champions)
            this._items.Add(new ChampionListItemViewModel(champion, favoritesService));

        this.BuildTagFilters(champions);
        this.BuildPartypeFilters();

        this.ChampionsView = CollectionViewSource.GetDefaultView(this._items);
        this.ChampionsView.Filter = this.ApplyFilter;
        this.ApplySort();

        logger.LogInformation(
            "Vue initialisée — {Count} champion(s), {Tags} classes, {Partypes} ressources",
            this._items.Count, this._tagFilters.Count, this._partypeFilters.Count);
    }

    // ── Construction des filtres ──────────────────────────────────────────

    private void BuildTagFilters(IEnumerable<Champion> champions)
    {
        var presentTags = champions
            .SelectMany(c => c.Tags ?? Enumerable.Empty<string>())
            .Distinct()
            .ToHashSet();

        var ordered = ChampionTags.CanonicalOrder
            .Where(t => presentTags.Contains(t))
            .Concat(presentTags.Except(ChampionTags.CanonicalOrder).OrderBy(t => t));

        this._tagFilters = ordered.Select(tag => new FilterItemViewModel(tag)).ToList();
        this.SubscribeToFilterItems(this._tagFilters);
        this.OnPropertyChanged(nameof(TagFilters));
        logger.LogDebug("TagFilters construits : {Tags}", string.Join(", ", this._tagFilters.Select(f => f.Label)));
    }

    private void BuildPartypeFilters()
    {
        this._partypeFilters = ChampionResources.CanonicalOrder
            .Select(category => new FilterItemViewModel(category))
            .ToList();
        this.SubscribeToFilterItems(this._partypeFilters);
        this.OnPropertyChanged(nameof(PartypeFilters));
    }

    private void SubscribeToFilterItems(IEnumerable<FilterItemViewModel> items)
    {
        foreach (var item in items)
            item.PropertyChanged += this.OnFilterItemChanged;
    }

    private void OnFilterItemChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterItemViewModel.IsSelected))
        {
            logger.LogDebug("Filtre item modifié : '{Label}' = {IsSelected}",
                (sender as FilterItemViewModel)?.Label,
                (sender as FilterItemViewModel)?.IsSelected);
            this.ChampionsView?.Refresh();
        }
    }

    // ── Tri ───────────────────────────────────────────────────────────────

    private void ApplySort()
    {
        if (this.ChampionsView is not ListCollectionView listView) return;

        logger.LogDebug("Application du tri : {Sort}", this.SelectedSortOption.Value);

        listView.CustomSort = this.SelectedSortOption.Key switch
        {
            SortOption.NomAZ => Comparer<ChampionListItemViewModel>.Create(
                (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Champion.Name, b.Champion.Name)),

            SortOption.NomZA => Comparer<ChampionListItemViewModel>.Create(
                (a, b) => StringComparer.OrdinalIgnoreCase.Compare(b.Champion.Name, a.Champion.Name)),

            SortOption.DifficulteAsc => Comparer<ChampionListItemViewModel>.Create(
                (a, b) => (a.Champion.Info?.Difficulty ?? 0).CompareTo(b.Champion.Info?.Difficulty ?? 0)),

            SortOption.DifficulteDesc => Comparer<ChampionListItemViewModel>.Create(
                (a, b) => (b.Champion.Info?.Difficulty ?? 0).CompareTo(a.Champion.Info?.Difficulty ?? 0)),

            _ => null,
        };
    }

    // ── Filtre central ────────────────────────────────────────────────────

    private bool ApplyFilter(object obj)
    {
        if (obj is not ChampionListItemViewModel item) return false;

        if (this.ShowFavoritesOnly && !item.IsFavorite) return false;
        if (this.SelectedDamageType != DamageTypeFilter.Tous && item.DamageType != this.SelectedDamageType) return false;
        if (this.SelectedRangeType == RangeTypeFilter.Melee &&  item.IsRanged) return false;
        if (this.SelectedRangeType == RangeTypeFilter.Range && !item.IsRanged) return false;

        var difficulty = item.Champion.Info?.Difficulty ?? 0;
        if (difficulty < this.DifficultyMin || difficulty > this.DifficultyMax) return false;

        var selectedTags = this._tagFilters.Where(f => f.IsSelected).ToList();
        if (selectedTags.Count > 0)
        {
            var championTags = item.Champion.Tags ?? [];
            if (!selectedTags.Any(f => championTags.Contains(f.Label))) return false;
        }

        var selectedPartypes = this._partypeFilters.Where(f => f.IsSelected).ToList();
        if (selectedPartypes.Count > 0)
        {
            var category = ChampionResources.GetCategory(item.Champion.Partype);
            if (!selectedPartypes.Any(f => f.Label == category)) return false;
        }

        if (!string.IsNullOrEmpty(this.NameFilter) &&
            !(item.Champion.Name?.Contains(this.NameFilter, StringComparison.OrdinalIgnoreCase) ?? false))
            return false;

        return true;
    }
}
