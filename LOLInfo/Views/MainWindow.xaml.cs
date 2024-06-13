namespace LOLInfo
{
    using System.Runtime.CompilerServices;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;

    using LOLInfo.Models.RiotModel;
    using Image = System.Windows.Controls.Image;
    using LOLInfo.IViewModels;
    using LOLInfo.ViewModels;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IMainViewModel _mainViewModel = new MainViewModel();
        public ObservableCollection<Champion> Champions = [];

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            GetAllChampion();
            ChampionItemsControl.ItemsSource = Champions;
        }

        private async void GetAllChampion()
        {
            var champions = await this._mainViewModel.GetAllChampions();
            foreach (var champion in champions)
            {
                this.Champions?.Add(champion);
            }

            Console.WriteLine("Control");
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            if (img != null)
            {
                BitmapImage bitmapImage = new BitmapImage
                {
                    UriSource = new Uri("ms-appx:///Assets/load01.gif")
                };
                img.Source = bitmapImage;
            }
        }
    }
}