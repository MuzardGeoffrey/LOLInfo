namespace LOLInfo.Views;

using System.Windows;
using System.Windows.Controls;

using LOLInfo.IServices;
using LOLInfo.IViewModels;

/// <summary>
/// Logique d'interaction pour DetailChampionPage.xaml
/// </summary>
public partial class DetailChampionPage : Page
{
    private readonly IDetailChampionViewModel _viewModel;
    private readonly IViewManager _viewManager;

    public DetailChampionPage(IViewManager viewManager, IDetailChampionViewModel viewModel)
    {
        this.InitializeComponent();
        this._viewModel   = viewModel;
        this._viewManager = viewManager;
        this.DataContext  = this._viewModel;
        this.Loaded += this.OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await this._viewModel.LoadAsync();
    }

    private void BackButton_OnClick(object sender, RoutedEventArgs e)
    {
        this._viewManager.NavigateToAllChampion();
    }

    private void NextSkinButton_OnClick(object sender, RoutedEventArgs e)
    {
        this._viewModel.SelectNextSkin();
    }

    private void PreviousSkinButton_OnClick(object sender, RoutedEventArgs e)
    {
        this._viewModel.SelectPreviousSkin();
    }
}
