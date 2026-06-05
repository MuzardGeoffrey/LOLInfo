namespace LOLInfo.Views;

using System.Windows;
using System.Windows.Controls;

using LOLInfo.IViewModels;
using LOLInfo.ViewModels;

/// <summary>
/// Navigateur d'objets réutilisable (recherche + filtres + grille d'icônes).
/// Le DataContext attendu est un <see cref="IItemsViewModel"/> ; cliquer une icône
/// définit son <see cref="IItemsViewModel.SelectedItem"/>.
/// </summary>
public partial class ItemBrowserControl : UserControl
{
    public ItemBrowserControl() => this.InitializeComponent();

    private void ItemButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ItemViewModel item }
            && this.DataContext is IItemsViewModel vm)
        {
            vm.SelectedItem = item;
        }
    }
}
