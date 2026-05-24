namespace LOLInfo.Services
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;

    using LOLInfo.IViewModels;
    using LOLInfo.ViewModels;
    using LOLInfo.Views;

    using Microsoft.Extensions.DependencyInjection;

    public class ViewManager : IViewManager
    {
        private Page _currentPage;

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
            this.CurrentPage = new AllChampionPage(
                App.Current.Services.GetRequiredService<IViewManager>(),
                App.Current.Services.GetRequiredService<IAllChampionViewModel>()
            );
        }

        public void NavigateToDetail(string championName)
        {
            // DetailChampionViewModel a besoin de championName au moment de la navigation,
            // donc on l'instancie directement avec ses dépendances résolues via DI.
            var detailViewModel = new DetailChampionViewModel(
                App.Current.Services.GetRequiredService<IViewManager>(),
                App.Current.Services.GetRequiredService<IRiotClient>(),
                championName
            );

            this.CurrentPage = new DetailChampionPage(
                App.Current.Services.GetRequiredService<IViewManager>(),
                detailViewModel
            );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}