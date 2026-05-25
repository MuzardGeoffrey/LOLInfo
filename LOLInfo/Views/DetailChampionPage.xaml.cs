namespace LOLInfo.Views
{
    using System.Windows;
    using System.Windows.Controls;

    using LOLInfo.IViewModels;
    using LOLInfo.Services;

    /// <summary>
    /// Logique d'interaction pour DetailChampionPage.xaml
    /// </summary>
    public partial class DetailChampionPage : Page
    {
        private readonly IDetailChampionViewModel _viewModel;
        private readonly IViewManager _viewManager;

        public DetailChampionPage(IViewManager viewManager, IDetailChampionViewModel viewModel)
        {
            InitializeComponent();
            _viewModel   = viewModel;
            _viewManager = viewManager;
            DataContext  = _viewModel;

            // Même pattern que AllChampionPage :
            // LoadAsync() est déclenché une seule fois quand la page est affichée.
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadAsync();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _viewManager.NavigateToAllChampion();
        }
    }
}
