namespace LOLInfo.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    using LOLInfo.IViewModels;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;

    using Image = System.Windows.Controls.Image;

    /// <summary>
    /// Logique d'interaction pour AllChampionPage.xaml
    /// </summary>
    public partial class AllChampionPage : Page
    {
        private readonly IAllChampionPageViewModel _allChampionPageViewModel = new AllChampionPageViewModel();
        public ObservableCollection<Champion> Champions = [];

        public AllChampionPage()
        {
            InitializeComponent();
            this.DataContext = this;
            GetAllChampion();
            ChampionItemsControl.ItemsSource = Champions;
        }

        private async void GetAllChampion()
        {
            var champions = await this._allChampionPageViewModel.GetAllChampions();
            foreach (var champion in champions)
            {
                this.Champions?.Add(champion);
            }

            Console.WriteLine("Control");
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Image img) return;
            var bitmapImage = new BitmapImage
            {
                UriSource = new Uri("ms-appx:///Assets/load01.gif")
            };
            img.Source = bitmapImage;
        }

        private void ButtonChampion_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DetailChampionPage());
        }
    }
}