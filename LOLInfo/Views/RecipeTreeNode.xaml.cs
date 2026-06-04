namespace LOLInfo.Views;

using System.Windows.Controls;

/// <summary>
/// Contrôle récursif dessinant l'arbre de fabrication d'un objet
/// (cf. <see cref="ViewModels.ItemRecipeNode"/>). Voir RecipeTreeNode.xaml.
/// </summary>
public partial class RecipeTreeNode : UserControl
{
    public RecipeTreeNode() => this.InitializeComponent();
}
