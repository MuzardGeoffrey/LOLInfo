namespace LOLInfo.Services;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

using LOLInfo.IServices;
using LOLInfo.IViewModels;
using LOLInfo.ViewModels;
using LOLInfo.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ViewManager(ILogger<ViewManager> logger) : IViewManager
{
    private Page _currentPage;

    public Page CurrentPage
    {
        get => this._currentPage;
        private set { this._currentPage = value; this.OnPropertyChanged(nameof(CurrentPage)); }
    }

    public void NavigateToAllChampion()
    {
        logger.LogInformation("Navigation vers AllChampionPage");
        this.CurrentPage = new AllChampionPage(
            App.Current.Services.GetRequiredService<IViewManager>(),
            App.Current.Services.GetRequiredService<IAllChampionViewModel>());
        logger.LogDebug("AllChampionPage instanciée et définie comme page courante");
    }

    public void NavigateToDetail(string championName)
    {
        logger.LogInformation("Navigation vers DetailChampionPage — champion : '{ChampionName}'", championName);

        var detailViewModel = new DetailChampionViewModel(
            App.Current.Services.GetRequiredService<IViewManager>(),
            App.Current.Services.GetRequiredService<IRiotClient>(),
            App.Current.Services.GetRequiredService<ICdragonClient>(),
            championName,
            App.Current.Services.GetRequiredService<ILogger<DetailChampionViewModel>>());

        this.CurrentPage = new DetailChampionPage(
            App.Current.Services.GetRequiredService<IViewManager>(),
            detailViewModel);

        logger.LogDebug("DetailChampionPage instanciée pour '{ChampionName}'", championName);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
