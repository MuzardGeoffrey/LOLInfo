namespace LOLInfo.IViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;

    using LOLInfo.Models;

    public interface IAllChampionViewModel
    {
        /// <summary>
        /// Vue triée et filtrée de la liste des champions.
        /// Lier l'ItemsControl à cette propriété.
        /// </summary>
        ICollectionView ChampionsView { get; }

        /// <summary>Options de tri disponibles (enum + libellé).</summary>
        List<KeyValuePair<SortOption, string>> SortOptions { get; }

        /// <summary>Option de tri actuellement sélectionnée.</summary>
        KeyValuePair<SortOption, string> SelectedSortOption { get; set; }

        /// <summary>
        /// Quand true, la CollectionView n'affiche que les champions en favori.
        /// </summary>
        bool ShowFavoritesOnly { get; set; }

        Task GetAllChampions();
    }
}
