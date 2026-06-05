namespace LOLInfo.Views;

using System.Windows.Controls;

/// <summary>
/// Panneau de détail d'un objet (icône, coût, stats, effets, recette).
/// Le DataContext attendu est un <see cref="ViewModels.ItemViewModel"/> (ou null).
/// </summary>
public partial class ItemDetailControl : UserControl
{
    public ItemDetailControl() => this.InitializeComponent();
}
