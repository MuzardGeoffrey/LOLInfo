namespace LOLInfo.Views
{
    using System.Windows;
    using System.Windows.Controls;

    using LOLInfo.IViewModels;
    using LOLInfo.IViews;
    using LOLInfo.Services;

    /// <summary>
    /// Logique d'interaction pour DetailChampionPage.xaml
    /// </summary>
    public partial class DetailChampionPage : Page, IDetailChampionView
    {
        private readonly IDetailChampionViewModel _viewModel;
        private readonly IViewManager _viewManager;

        public DetailChampionPage(IViewManager viewManager, IDetailChampionViewModel viewModel)
        {
            this.InitializeComponent();
            _viewModel = viewModel;
            _viewManager = viewManager;
            this.DataContext = _viewModel;
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _viewManager.NavigateToAllChampion();
        }
    }
}