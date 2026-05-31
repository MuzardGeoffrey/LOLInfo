namespace LOLInfo.IViewModels;

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;

using LOLInfo.Models;
using LOLInfo.ViewModels;

public interface IAllChampionViewModel
{
    /// <summary>Vue triée et filtrée de la liste des champions (vide avant le chargement).</summary>
    ICollectionView ChampionsView { get; }

    // ── Tri ──────────────────────────────────────────────────────────────

    List<KeyValuePair<SortOption, string>> SortOptions { get; }
    KeyValuePair<SortOption, string> SelectedSortOption { get; set; }

    // ── Filtres simples ───────────────────────────────────────────────────

    string NameFilter         { get; set; }
    bool   ShowFavoritesOnly  { get; set; }
    DamageTypeFilter SelectedDamageType { get; set; }
    RangeTypeFilter  SelectedRangeType  { get; set; }

    // ── Filtres multi-sélection ───────────────────────────────────────────

    IReadOnlyList<FilterItemViewModel> TagFilters     { get; }
    IReadOnlyList<FilterItemViewModel> PartypeFilters { get; }

    // ── Filtre difficulté ─────────────────────────────────────────────────

    int DifficultyMin { get; set; }
    int DifficultyMax { get; set; }

    Task GetAllChampions();
}
