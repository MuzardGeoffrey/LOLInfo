namespace LOLInfo.Services
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;

    using LOLInfo.IViewModels;
    using LOLInfo.Services.Storage;
    using LOLInfo.ViewModels;
    using LOLInfo.Views;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    public class ViewManager : IViewManager
    {
        private readonly ILogger<ViewManager> _logger;
        private Page _currentPage;

        public ViewManager(ILogger<ViewManager> logger)
        {
            _logger = logger;
        }

        public Page CurrentPage
        {
            get => _currentPage;
            private set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public void NavigateToAllChampion()
        {
            _logger.LogInformation("Navigation vers AllChampionPage");

            this.CurrentPage = new AllChampionPage(
                App.Current.Services.GetRequiredService<IViewManager>(),
                App.Current.Services.GetRequiredService<IAllChampionViewModel>()
            );

            _logger.LogDebug("AllChampionPage instanciée et définie comme page courante");
        }
        // Note : IAllChampionViewModel est résolu via le DI (AddTransient).
        // AllChampionViewModel reçoit IFavoritesService en injection automatique.

        public void NavigateToDetail(string championName)
        {
            _logger.LogInformation("Navigation vers DetailChampionPage — champion : '{ChampionName}'", championName);

            // DetailChampionViewModel a besoin de championName au moment de la navigation,
            // donc on l'instancie directement avec ses dépendances résolues via DI.
            var detailViewModel = new DetailChampionViewModel(
                App.Current.Services.GetRequiredService<IViewManager>(),
                App.Current.Services.GetRequiredService<IRiotClient>(),
                championName,
                App.Current.Services.GetRequiredService<ILogger<DetailChampionViewModel>>()
            );

            this.CurrentPage = new DetailChampionPage(
                App.Current.Services.GetRequiredService<IViewManager>(),
                detailViewModel
            );

            _logger.LogDebug("DetailChampionPage instanciée pour '{ChampionName}' et définie comme page courante", championName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}