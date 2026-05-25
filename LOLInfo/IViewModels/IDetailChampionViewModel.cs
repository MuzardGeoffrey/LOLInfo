namespace LOLInfo.IViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;

    public interface IDetailChampionViewModel
    {
        /// <summary>Nom du champion, défini à la navigation.</summary>
        string ChampionName { get; }

        /// <summary>
        /// Champion chargé depuis l'API. Null jusqu'à la fin du chargement.
        /// La View bind directement sur les propriétés de Champion.
        /// </summary>
        Champion? Champion { get; }

        /// <summary>
        /// Sorts du champion (Passif, Q, W, E, R) prêts pour l'affichage.
        /// Construit après le chargement. Vide tant que Champion est null.
        /// </summary>
        IReadOnlyList<SpellViewModel> Spells { get; }

        /// <summary>
        /// Déclenche le chargement des données depuis l'API.
        /// Appelé depuis l'événement Loaded de la Page.
        /// </summary>
        Task LoadAsync();
    }
}
