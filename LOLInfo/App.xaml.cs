namespace LOLInfo
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Threading;

    using LOLInfo.IViewModels;
    using LOLInfo.Services;
    using LOLInfo.Services.Storage;
    using LOLInfo.ViewModels;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Serilog;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeSerilog();
            Services = ConfigureServices();
            this.InitializeComponent();
        }

        /// <summary>Gets the current <see cref="App"/> instance in use.</summary>
        public new static App Current => (App)Application.Current;

        /// <summary>Gets the <see cref="IServiceProvider"/> instance to resolve application services.</summary>
        public IServiceProvider Services { get; }

        // ── Logging ──────────────────────────────────────────────────────

        /// <summary>
        /// Configure Serilog avant la construction du DI.
        /// - Fichier : %AppData%\LOLInfo\logs\lolinfo-YYYYMMDD.log
        /// - Rotation quotidienne, 7 fichiers conservés.
        /// - SourceContext affiché pour identifier la classe qui logue.
        /// </summary>
        private static void InitializeSerilog()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LOLInfo", "logs");

            Directory.CreateDirectory(logDirectory);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "lolinfo-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} — {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("=== LOLInfo démarrage ===");
        }

        // ── DI ───────────────────────────────────────────────────────────

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Logging : ILogger<T> disponible dans tous les services via injection
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            // Services réseau
            services.AddSingleton<IRiotClient, RiotClient>();

            // Stockage local
            services.AddSingleton<IFavoritesService, FavoritesService>();

            // Navigation
            services.AddSingleton<IViewManager, ViewManager>();

            // ViewModels
            services.AddTransient<IAllChampionViewModel, AllChampionViewModel>();
            services.AddSingleton<MainViewModel>();

            return services.BuildServiceProvider();
        }

        // ── Cycle de vie ──────────────────────────────────────────────────

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Capture les exceptions non gérées sur le thread UI
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Capture les exceptions non gérées sur les threads background
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            Log.Information("Navigation initiale vers AllChampionPage");
            Services.GetRequiredService<IViewManager>().NavigateToAllChampion();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("=== LOLInfo arrêt (code {ExitCode}) ===", e.ApplicationExitCode);
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        // ── Handlers exceptions non gérées ───────────────────────────────

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "Exception non gérée sur le thread UI");
            // On laisse l'application crasher (e.Handled = false par défaut)
            // pour ne pas masquer des bugs — le log est suffisant pour diagnostiquer.
        }

        private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.ExceptionObject as Exception, "Exception non gérée sur un thread background (IsTerminating={IsTerminating})", e.IsTerminating);
            Log.CloseAndFlush();
        }
    }
}
