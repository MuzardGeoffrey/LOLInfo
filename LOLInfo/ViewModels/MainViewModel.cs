namespace LOLInfo.ViewModels
{
    using System.ComponentModel;
    using System.Windows.Controls;

    using LOLInfo.Services;

    public class MainViewModel : BaseViewModel
    {
        private readonly IViewManager _viewManager;

        public MainViewModel(IViewManager viewManager)
        {
            _viewManager = viewManager;
            _viewManager.PropertyChanged += OnViewManagerPropertyChanged;
        }

        /// <summary>
        /// Page courante affichée dans le Frame de MainWindow.
        /// Délègue à ViewManager qui gère la navigation.
        /// </summary>
        public Page CurrentPage => _viewManager.CurrentPage;

        private void OnViewManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IViewManager.CurrentPage))
                OnPropertyChanged(nameof(CurrentPage));
        }
    }
}