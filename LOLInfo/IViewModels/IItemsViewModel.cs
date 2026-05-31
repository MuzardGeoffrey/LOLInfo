namespace LOLInfo.IViewModels;

using System.ComponentModel;
using System.Threading.Tasks;

using LOLInfo.ViewModels;

public interface IItemsViewModel : INotifyPropertyChanged
{
    /// <summary>Vue filtrée des objets (recherche par nom).</summary>
    ICollectionView ItemsView { get; }

    /// <summary>Filtre de recherche par nom.</summary>
    string NameFilter { get; set; }

    /// <summary>Objet sélectionné (affiché dans le panneau de détail).</summary>
    ItemViewModel? SelectedItem { get; set; }

    /// <summary>True une fois les objets chargés.</summary>
    bool IsLoaded { get; }

    /// <summary>Charge les objets depuis l'API (idempotent).</summary>
    Task LoadAsync();
}
