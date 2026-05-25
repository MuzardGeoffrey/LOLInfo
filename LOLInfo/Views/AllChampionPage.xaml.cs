namespace LOLInfo.Views
{
    using System.Windows;
    using System.Windows.Controls;

    using LOLInfo.IViewModels;
    using LOLInfo.IViews;
    using LOLInfo.Services;
    using LOLInfo.ViewModels;

    /// <summary>
    /// Logique d'interaction pour AllChampionPage.xaml
    /// </summary>
    public partial class AllChampionPage : Page, IAllChampionView
    {
        private readonly IViewManager _viewManager;
        private readonly IAllChampionViewModel _viewModel;

        public AllChampionPage(IViewManager viewManager, IAllChampionViewModel allChampionViewModel)
        {
            this.InitializeComponent();
            this._viewManager = viewManager;
            this._viewModel = allChampionViewModel;
            this.DataContext = this._viewModel;
            _ = this._viewModel.GetAllChampions();
        }

        private void ButtonChampion_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;

            // Le DataContext est maintenant un ChampionListItemViewModel,
            // pas directement un Champion.
            if (button.DataContext is not ChampionListItemViewModel item) return;

            if (item.Champion.Id != null)
                this._viewManager.NavigateToDetail(item.Champion.Id);
        }
    }
}
