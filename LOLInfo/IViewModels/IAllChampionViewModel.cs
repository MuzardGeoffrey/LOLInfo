namespace LOLInfo.IViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;

    using LOLInfo.Models;

    public interface IAllChampionViewModel
    {
        /// <summary>
        /// Vue triée (et plus tard filtrée) de la collection de champions.
        /// Lier l'ItemsControl à cette propriété plutôt qu'à Champions directement.
        /// </summary>
        ICollectionView ChampionsView { get; }

        /// <summary>
        /// Liste des options de tri disponibles (clé enum + libellé affiché).
        /// </summary>
        List<KeyValuePair<SortOption, string>> SortOptions { get; }

        /// <summary>
        /// Option de tri sélectionnée. Le setter déclenche ApplySort().
        /// </summary>
        KeyValuePair<SortOption, string> SelectedSortOption { get; set; }

        Task GetAllChampions();
    }
}
