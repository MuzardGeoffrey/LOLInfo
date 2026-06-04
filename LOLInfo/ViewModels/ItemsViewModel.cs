namespace LOLInfo.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

using LOLInfo.IServices;
using LOLInfo.IViewModels;
using LOLInfo.Models;
using LOLInfo.Properties;

using Microsoft.Extensions.Logging;

/// <summary>
/// Liste des objets avec recherche par nom, filtre par mode de jeu, tri
/// (nom, coût, statistique) et sélection (panneau de détail).
/// Mirroir simplifié d'<see cref="AllChampionViewModel"/>.
/// </summary>
public class ItemsViewModel : BaseViewModel, IItemsViewModel
{
    private readonly IItemClient _client;
    private readonly ILogger<ItemsViewModel> _logger;

    private List<ItemViewModel> _items = [];

    private ICollectionView _itemsView =
        CollectionViewSource.GetDefaultView(new List<ItemViewModel>());

    public ItemsViewModel(IItemClient client, ILogger<ItemsViewModel> logger)
    {
        this._client = client;
        this._logger = logger;

        this.SortOptions =
        [
            new("name",      Resources.Sort_NameAsc),
            new("cost_asc",  Resources.ItemSort_CostAsc),
            new("cost_desc", Resources.ItemSort_CostDesc),
            .. SortableStats.Select(s => new ItemSortOption(s.Key, s.Label())),
        ];
        this.SelectedSortOption = this.SortOptions[0];

        // Filtres multi-sélection par statistique (mêmes stats que le tri).
        this._statFilters = SortableStats.Select(s => new FilterItemViewModel(s.Key, s.Label())).ToList();
        foreach (var filter in this._statFilters)
            filter.PropertyChanged += this.OnStatFilterChanged;
    }

    public ICollectionView ItemsView
    {
        get => this._itemsView;
        private set { this._itemsView = value; this.OnPropertyChanged(nameof(ItemsView)); }
    }

    public bool IsLoaded { get; private set; }

    public IReadOnlyList<ItemViewModel> AllItems => this._items;

    // ── Correspondance mode de jeu → identifiant de carte DataDragon ───────────
    private static readonly Dictionary<GameModeFilter, string> MapIds = new()
    {
        [GameModeFilter.NormalClasse] = "11", // Faille de l'invocateur (parties normales/classées)
        [GameModeFilter.Aram]   = "12", // ARAM (Abîme hurlant)
        [GameModeFilter.Arena]  = "30", // Arena
    };

    // ── Statistiques proposées au tri (clé brute DataDragon → libellé court) ───
    private static readonly (string Key, Func<string> Label)[] SortableStats =
    [
        ("FlatPhysicalDamageMod", () => Resources.Stat_AttackDamage),
        ("FlatMagicDamageMod",    () => Resources.Stat_AbilityPower),
        ("FlatHPPoolMod",         () => Resources.Stat_Health),
        ("FlatArmorMod",          () => Resources.Stat_Armor),
        ("FlatSpellBlockMod",     () => Resources.Stat_MagicResist),
        ("PercentAttackSpeedMod", () => Resources.Stat_AttackSpeed),
        ("FlatCritChanceMod",     () => Resources.Stat_CritChance),
        ("FlatMPPoolMod",         () => Resources.Stat_Mana),
        ("FlatMovementSpeedMod",  () => Resources.Stat_MovementSpeed),
        ("FlatHPRegenMod",        () => Resources.Stat_HealthRegen),
        ("FlatMPRegenMod",        () => Resources.Stat_ManaRegen),
    ];

    // ── Tri ────────────────────────────────────────────────────────────────────

    /// <summary>Options de tri : nom, coût croissant/décroissant, puis une par statistique.</summary>
    public IReadOnlyList<ItemSortOption> SortOptions { get; }

    public ItemSortOption SelectedSortOption
    {
        get;
        set
        {
            field = value;
            this.OnPropertyChanged(nameof(SelectedSortOption));
            this.ApplySort();
        }
    }

    // ── Filtres ──────────────────────────────────────────────────────────────

    public string NameFilter
    {
        get;
        set
        {
            field = value ?? string.Empty;
            this.OnPropertyChanged(nameof(NameFilter));
            this.ItemsView?.Refresh();
        }
    } = string.Empty;

    public GameModeFilter SelectedGameMode
    {
        get;
        set
        {
            field = value;
            this.OnPropertyChanged(nameof(SelectedGameMode));
            this._logger.LogDebug("Filtre mode de jeu : {Value}", value);
            this.RebuildKeptIds();
            this.ItemsView?.Refresh();
        }
    } = GameModeFilter.Tous;

    public ItemViewModel? SelectedItem
    {
        get;
        set { field = value; this.OnPropertyChanged(nameof(SelectedItem)); }
    }

    // ── Suggestions de recherche (autocomplétion) ──────────────────────────────

    private IReadOnlyList<ItemViewModel> _searchSuggestions = [];

    /// <summary>Objets proposés en autocomplétion : noms distincts, hors objets obsolètes.</summary>
    public IReadOnlyList<ItemViewModel> SearchSuggestions => this._searchSuggestions;

    // ── Filtre : statistiques (multi-sélection, ET) ────────────────────────────

    private readonly List<FilterItemViewModel> _statFilters;

    /// <summary>Cases à cocher de statistiques ; un objet doit posséder toutes celles cochées.</summary>
    public IReadOnlyList<FilterItemViewModel> StatFilters => this._statFilters;

    private void OnStatFilterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterItemViewModel.IsSelected))
            this.ItemsView?.Refresh();
    }

    public async Task LoadAsync()
    {
        if (this.IsLoaded) return;

        this._logger.LogDebug("Chargement des objets");
        var items = await this._client.GetAllItems();

        this._items = items.Select(ItemViewModel.From).ToList();

        // Index id → objet, puis construction de l'arbre de fabrication de chacun
        // (les composants se résolvent par leur identifiant via cet index).
        var byId = new Dictionary<string, ItemViewModel>();
        foreach (var item in this._items)
            if (!string.IsNullOrEmpty(item.Id)) byId[item.Id] = item;

        foreach (var item in this._items)
            item.SetRecipe(ItemRecipeNode.Build(item, byId));

        // Suggestions d'autocomplétion : un objet par nom, hors obsolètes, triés.
        this._searchSuggestions = this._items
            .Where(i => i.IsOnAnyMap)
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        this.OnPropertyChanged(nameof(SearchSuggestions));

        this.RebuildKeptIds();
        this.ItemsView = CollectionViewSource.GetDefaultView(this._items);
        this.ItemsView.Filter = this.ApplyFilter;
        this.ApplySort();
        this.SelectedItem = this.ItemsView.Cast<ItemViewModel>().FirstOrDefault();
        this.IsLoaded = true;

        this._logger.LogInformation("Objets chargés — {Count} affiché(s)", this._items.Count);
    }

    private bool ApplyFilter(object obj)
    {
        if (obj is not ItemViewModel item) return false;

        // Mode de jeu + déduplication : on ne garde qu'une variante par nom (cf. RebuildKeptIds).
        if (!this._keptIds.Contains(item.Id)) return false;

        if (!string.IsNullOrEmpty(this.NameFilter) &&
            !item.Name.Contains(this.NameFilter, StringComparison.OrdinalIgnoreCase))
            return false;

        // Statistiques (ET) : l'objet doit posséder chacune des stats cochées.
        foreach (var stat in this._statFilters)
            if (stat.IsSelected && !HasStat(item, stat.Key))
                return false;

        return true;
    }

    private static bool HasStat(ItemViewModel item, string statKey)
        => item.RawStats.TryGetValue(statKey, out var value) && value != 0;

    // ── Déduplication par mode ─────────────────────────────────────────────────
    // Riot publie un objet par mode de jeu (même nom, identifiants différents).
    // On ne conserve qu'une variante par nom, recalculée à chaque changement de mode.

    private const string RiftMapId = "11";

    private HashSet<string> _keptIds = [];

    private void RebuildKeptIds()
    {
        var candidates = MapIds.TryGetValue(this.SelectedGameMode, out var mapId)
            ? this._items.Where(i => i.IsAvailableOn(mapId))
            : this._items;

        this._keptIds = candidates
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .Select(this.ChooseRepresentative)
            .Where(i => i.IsOnAnyMap) // masque les objets obsolètes (retirés de toutes les cartes)
            .Select(i => i.Id)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Variante représentative d'un groupe de même nom : pour « Tous » on privilégie
    /// la version Faille, puis toute variante encore en jeu ; pour un mode donné, la
    /// moins chère. Le départage final se fait par coût puis identifiant.
    /// </summary>
    private ItemViewModel ChooseRepresentative(IGrouping<string, ItemViewModel> group)
    {
        IEnumerable<ItemViewModel> pool = group;
        if (this.SelectedGameMode == GameModeFilter.Tous)
        {
            var rift = group.Where(i => i.IsAvailableOn(RiftMapId)).ToList();
            if (rift.Count > 0)
                pool = rift;
            else
            {
                // Pas de version Faille : garder une variante encore présente sur une
                // carte (ex. objets exclusifs à l'Arena) plutôt qu'une variante morte.
                var live = group.Where(i => i.IsOnAnyMap).ToList();
                if (live.Count > 0) pool = live;
            }
        }

        return pool.OrderBy(i => i.Gold).ThenBy(i => i.Id, StringComparer.Ordinal).First();
    }

    // ── Tri ────────────────────────────────────────────────────────────────────

    private void ApplySort()
    {
        if (this.ItemsView is not ListCollectionView listView) return;

        var key = this.SelectedSortOption?.Key ?? "name";
        this._logger.LogDebug("Application du tri objets : {Sort}", key);

        listView.CustomSort = key switch
        {
            "name" => Comparer<ItemViewModel>.Create(
                (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name)),

            "cost_asc" => Comparer<ItemViewModel>.Create(
                (a, b) => a.Gold.CompareTo(b.Gold)),

            "cost_desc" => Comparer<ItemViewModel>.Create(
                (a, b) => b.Gold.CompareTo(a.Gold)),

            // Tri par statistique : valeur décroissante, objets sans la stat en dernier,
            // départage stable par nom.
            _ => Comparer<ItemViewModel>.Create((a, b) =>
            {
                var byStat = StatValue(b, key).CompareTo(StatValue(a, key));
                return byStat != 0 ? byStat : StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
            }),
        };
    }

    private static double StatValue(ItemViewModel item, string statKey)
        => item.RawStats.TryGetValue(statKey, out var v) ? v : 0;
}
