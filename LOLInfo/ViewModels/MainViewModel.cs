namespace LOLInfo.ViewModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;

    using LOLInfo.Views;

    public class MainViewModel : INotifyPropertyChanged
    {
        private Page _currentPage;

        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            CurrentPage = new AllChampionPage();
        }

        public void Navigate(Page page)
        {
            CurrentPage = page;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}