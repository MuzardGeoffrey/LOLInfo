namespace LOLInfo.IViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;

    using LOLInfo.Models;
    using LOLInfo.ViewModels;

    public interface IAllChampionViewModel
    {
        /// <summary>
        /// Vue triée et filtrée de la liste des champions.
        /// Lier l'ItemsControl à cette propriété.
        /// </summary>
        ICollectionView ChampionsView { get; }

        // ── Tri ──────────────────────────────────────────────────────────

        /// <summary>Options de tri disponibles (enum + libellé).</summary>
        List<KeyValuePair<SortOption, string>> SortOptions { get; }

        /// <summary>Option de tri actuellement sélectionnée.</summary>
        KeyValuePair<SortOption, string> SelectedSortOption { get; set; }

        // ── Filtres simples ───────────────────────────────────────────────

        /// <summary>Texte de recherche par nom. Vide = pas de filtre.</summary>
        string NameFilter { get; set; }

        /// <summary>Quand true, n'affiche que les champions en favori.</summary>
        bool ShowFavoritesOnly { get; set; }

        /// <summary>Filtre sur le type de dégâts principal (Tous / AD / AP / Mixte).</summary>
        DamageTypeFilter SelectedDamageType { get; set; }

        /// <summary>Filtre sur la portée (Tous / Melee / Range).</summary>
        RangeTypeFilter SelectedRangeType { get; set; }

        // ── Filtres multi-sélection ───────────────────────────────────────

        /// <summary>
        /// Liste des classes (Tags Riot) disponibles.
        /// Chaque item porte son propre IsSelected.
        /// Construit dynamiquement après le chargement des champions.
        /// Si aucun item n'est sélectionné, le filtre est inactif.
        /// </summary>
        IReadOnlyList<FilterItemViewModel> TagFilters { get; }

        /// <summary>
        /// Liste des types de ressources (Partype) disponibles.
        /// Même fonctionnement que TagFilters.
        /// </summary>
        IReadOnlyList<FilterItemViewModel> PartypeFilters { get; }

        // ── Filtre difficulté ─────────────────────────────────────────────

        /// <summary>Borne inférieure du filtre difficulté (0-10).</summary>
        int DifficultyMin { get; set; }

        /// <summary>Borne supérieure du filtre difficulté (0-10).</summary>
        int DifficultyMax { get; set; }

        Task GetAllChampions();
    }
}
