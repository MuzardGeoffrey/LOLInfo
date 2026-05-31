namespace LOLInfo.Services;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

using LOLInfo.IServices;
using LOLInfo.IViewModels;
using LOLInfo.ViewModels;
using LOLInfo.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ViewManager(IServiceProvider services, ILogger<ViewManager> logger) : IViewManager
{
    private Page? _currentPage;

    public Page? CurrentPage
    {
        get => this._currentPage;
        private set { this._currentPage = value; this.OnPropertyChanged(nameof(CurrentPage)); }
    }

    public void NavigateToAllChampion()
    {
        logger.LogInformation("Navigation vers AllChampionPage");
        this.CurrentPage = new AllChampionPage(
            this,
            services.GetRequiredService<IAllChampionViewModel>());
        logger.LogDebug("AllChampionPage instanciée et définie comme page courante");
    }

    public void NavigateToDetail(string championName)
    {
        logger.LogInformation("Navigation vers DetailChampionPage — champion : '{ChampionName}'", championName);

        // championName est un argument runtime : ActivatorUtilities résout les autres
        // dépendances (IRiotClient, ICdragonClient, ILogger) depuis le conteneur.
        var detailViewModel = ActivatorUtilities.CreateInstance<DetailChampionViewModel>(
            services, championName);

        this.CurrentPage = new DetailChampionPage(this, detailViewModel);

        logger.LogDebug("DetailChampionPage instanciée pour '{ChampionName}'", championName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
