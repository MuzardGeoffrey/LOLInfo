namespace LOLInfo
{
    using System.Windows;

    using LOLInfo.ViewModels;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext résolu depuis le conteneur DI (MainViewModel délègue CurrentPage à IViewManager)
            this.DataContext = App.Current.Services.GetRequiredService<MainViewModel>();
        }

        /// <summary>Sélectionne l'objet cliqué dans la grille (panneau de détail).</summary>
        private void ItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ItemViewModel item }
                && this.DataContext is MainViewModel main)
            {
                main.Items.SelectedItem = item;
            }
        }
    }
}
