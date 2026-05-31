namespace LOLInfo.ViewModels;

using System.ComponentModel;
using System.Windows.Controls;

using LOLInfo.IServices;

public class MainViewModel : BaseViewModel
{
    private readonly IViewManager _viewManager;

    public MainViewModel(IViewManager viewManager)
    {
        this._viewManager = viewManager;
        this._viewManager.PropertyChanged += this.OnViewManagerPropertyChanged;
    }

    public Page? CurrentPage => this._viewManager.CurrentPage;

    private void OnViewManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IViewManager.CurrentPage))
            this.OnPropertyChanged(nameof(CurrentPage));
    }
}
