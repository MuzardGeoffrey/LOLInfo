namespace LOLInfo.IViewModels;

using System.Collections.Generic;
using System.Threading.Tasks;

using LOLInfo.Models.RiotModel;
using LOLInfo.ViewModels;

public interface IDetailChampionViewModel
{
    /// <summary>Nom du champion, défini à la navigation.</summary>
    string ChampionName { get; }

    /// <summary>Champion chargé depuis l'API. Null jusqu'à la fin du chargement.</summary>
    Champion? Champion { get; }

    /// <summary>Sorts du champion (Passif, Q, W, E, R) prêts pour l'affichage.</summary>
    IReadOnlyList<SpellViewModel> Spells { get; }

    /// <summary>Skins du champion.</summary>
    IReadOnlyList<SkinViewModel> Skins { get; }

    /// <summary>True si au moins un skin est disponible.</summary>
    bool HasSkins { get; }

    /// <summary>Skin affiché au centre du carrousel.</summary>
    SkinViewModel? SelectedSkin { get; set; }

    /// <summary>Aperçu du skin précédent (boucle). Null si moins de 2 skins.</summary>
    SkinViewModel? PreviousSkin { get; }

    /// <summary>Aperçu du skin suivant (boucle). Null si moins de 2 skins.</summary>
    SkinViewModel? NextSkin { get; }

    /// <summary>Niveaux sélectionnables (1 à 18).</summary>
    IReadOnlyList<int> Levels { get; }

    /// <summary>Niveau choisi pour le calcul des stats.</summary>
    int SelectedLevel { get; set; }

    /// <summary>Stats du champion au niveau sélectionné (objets équipés inclus).</summary>
    IReadOnlyList<ChampionStatRow> ChampionStats { get; }

    /// <summary>Navigateur d'objets partagé (grille + filtres + détail) pour la sélection.</summary>
    IItemsViewModel Items { get; }

    /// <summary>Objets actuellement équipés.</summary>
    System.Collections.ObjectModel.ObservableCollection<ItemViewModel> EquippedItems { get; }

    /// <summary>True s'il reste de la place pour un objet.</summary>
    bool CanEquipMore { get; }

    /// <summary>Équipe l'objet sélectionné dans le navigateur (<see cref="Items"/>).</summary>
    void EquipCurrent();

    /// <summary>Retire un objet équipé.</summary>
    void Unequip(ItemViewModel item);

    /// <summary>Déclenche le chargement des données depuis l'API.</summary>
    Task LoadAsync();

    /// <summary>Passe au skin suivant (boucle).</summary>
    void SelectNextSkin();

    /// <summary>Passe au skin précédent (boucle).</summary>
    void SelectPreviousSkin();
}
