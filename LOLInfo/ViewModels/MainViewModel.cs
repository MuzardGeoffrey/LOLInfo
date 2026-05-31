namespace LOLInfo.ViewModels;

using System.ComponentModel;
using System.Windows.Controls;

using LOLInfo.IServices;
using LOLInfo.IViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IViewManager _viewManager;

    public MainViewModel(IViewManager viewManager, IItemsViewModel items)
    {
        this._viewManager = viewManager;
        this.Items = items;
        this._viewManager.PropertyChanged += this.OnViewManagerPropertyChanged;
    }

    public Page? CurrentPage => this._viewManager.CurrentPage;

    /// <summary>ViewModel de l'onglet Objets.</summary>
    public IItemsViewModel Items { get; }

    private void OnViewManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IViewManager.CurrentPage))
            this.OnPropertyChanged(nameof(CurrentPage));
    }
}
