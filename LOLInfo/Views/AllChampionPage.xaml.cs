namespace LOLInfo.Views;

using System.Windows;
using System.Windows.Controls;

using LOLInfo.IServices;
using LOLInfo.IViewModels;
using LOLInfo.ViewModels;

/// <summary>
/// Logique d'interaction pour AllChampionPage.xaml
/// </summary>
public partial class AllChampionPage : Page
{
    private readonly IViewManager _viewManager;
    private readonly IAllChampionViewModel _viewModel;

    public AllChampionPage(IViewManager viewManager, IAllChampionViewModel allChampionViewModel)
    {
        this.InitializeComponent();
        this._viewManager = viewManager;
        this._viewModel   = allChampionViewModel;
        this.DataContext  = this._viewModel;
        this._viewModel.GetAllChampions();
    }

    private void ButtonChampion_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not ChampionListItemViewModel item) return;
        if (item.Champion.Id != null)
            this._viewManager.NavigateToDetail(item.Champion.Id);
    }
}
