namespace LOLInfo.IViewModels;

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using LOLInfo.Models;
using LOLInfo.ViewModels;

public interface IItemsViewModel : INotifyPropertyChanged
{
    /// <summary>Vue filtrée des objets (recherche par nom).</summary>
    ICollectionView ItemsView { get; }

    /// <summary>Tous les objets chargés (pour l'équipement sur un champion).</summary>
    IReadOnlyList<ItemViewModel> AllItems { get; }

    /// <summary>Filtre de recherche par nom.</summary>
    string NameFilter { get; set; }

    /// <summary>Filtre par mode de jeu (carte).</summary>
    GameModeFilter SelectedGameMode { get; set; }

    /// <summary>Filtres de statistiques (multi-sélection, combinés en ET).</summary>
    IReadOnlyList<FilterItemViewModel> StatFilters { get; }

    /// <summary>Options de tri proposées (nom, coût, par statistique).</summary>
    IReadOnlyList<ItemSortOption> SortOptions { get; }

    /// <summary>Option de tri sélectionnée.</summary>
    ItemSortOption SelectedSortOption { get; set; }

    /// <summary>Objet sélectionné (affiché dans le panneau de détail).</summary>
    ItemViewModel? SelectedItem { get; set; }

    /// <summary>True une fois les objets chargés.</summary>
    bool IsLoaded { get; }

    /// <summary>Charge les objets depuis l'API (idempotent).</summary>
    Task LoadAsync();
}
