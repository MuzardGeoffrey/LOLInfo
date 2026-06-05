namespace LOLInfo
{
    using System.Windows;
    using System.Windows.Controls;

    using LOLInfo.IServices.Storage;
    using LOLInfo.ViewModels;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Évite de re-déclencher la logique quand on rétablit la sélection après annulation.
        private bool _suppressLanguageChange;

        public MainWindow()
        {
            InitializeComponent();
            // DataContext résolu depuis le conteneur DI (MainViewModel délègue CurrentPage à IViewManager)
            this.DataContext = App.Current.Services.GetRequiredService<MainViewModel>();
        }

        /// <summary>
        /// Changement de langue : persiste le choix et propose un redémarrage (la langue
        /// d'affichage et des données n'est pleinement appliquée qu'au prochain démarrage).
        /// </summary>
        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._suppressLanguageChange) return;
            if (sender is not ComboBox combo || combo.SelectedItem is not LanguageOption option) return;

            var language = App.Current.Services.GetRequiredService<ILanguageService>();
            if (option.Code == language.CurrentLanguage) return; // présélection initiale

            var confirmed = MessageBox.Show(
                Properties.Resources.Language_RestartPrompt,
                Properties.Resources.Language_RestartTitle,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information) == MessageBoxResult.OK;

            if (confirmed)
            {
                language.SetLanguage(option.Code);
                App.Current.Restart();
            }
            else
            {
                // Annulation : rétablit l'affichage de la langue courante.
                this._suppressLanguageChange = true;
                combo.SelectedValue = language.CurrentLanguage;
                this._suppressLanguageChange = false;
            }
        }
    }
}
