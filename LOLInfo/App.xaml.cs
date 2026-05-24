namespace LOLInfo
{
    using System.Windows;

    using LOLInfo.IViewModels;
    using LOLInfo.Services;
    using LOLInfo.ViewModels;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services réseau
            services.AddSingleton<IRiotClient, RiotClient>();

            // Navigation
            services.AddSingleton<IViewManager, ViewManager>();

            // ViewModels
            services.AddTransient<IAllChampionViewModel, AllChampionViewModel>();
            services.AddSingleton<MainViewModel>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Déclenche la navigation initiale avant que StartupUri n'affiche la MainWindow.
            // MainWindow lira CurrentPage depuis son constructeur via le DataContext.
            Services.GetRequiredService<IViewManager>().NavigateToAllChampion();
        }
    }
}