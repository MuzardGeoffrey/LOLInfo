namespace LOLInfo.Views
{
    using System.Windows;
    using System.Windows.Controls;

    using LOLInfo.IViewModels;
    using LOLInfo.IViews;
    using LOLInfo.Services;

    using Models.RiotModel;

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
            // Lancement du chargement async sans bloquer le thread UI
            _ = this._viewModel.GetAllChampions();
        }

        private void ButtonChampion_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;

            if (button.DataContext is not Champion champion) return;

            // On utilise l'Id (ex: "MissFortune") et non le Name ("Miss Fortune")
            // car l'URL de l'API Riot est basée sur l'Id du champion.
            if (champion.Id != null)
                this._viewManager.NavigateToDetail(champion.Id);
        }
    }
}