namespace LOLInfo;

using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

using LOLInfo.IServices;
using LOLInfo.IServices.Storage;
using LOLInfo.IViewModels;
using LOLInfo.Localization;
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
        this.Services = ConfigureServices();

        // Applique la langue mémorisée (ou le défaut au premier lancement) AVANT
        // tout rendu de page ou appel réseau localisé (locale des données Riot).
        var language = this.Services.GetRequiredService<ILanguageService>();
        AppLocalization.ApplyCulture(language.CurrentLanguage);

        this.InitializeComponent();
    }

    /// <summary>Gets the current <see cref="App"/> instance in use.</summary>
    public new static App Current => (App)Application.Current;

    /// <summary>Gets the <see cref="IServiceProvider"/> instance to resolve application services.</summary>
    public IServiceProvider Services { get; }

    // ── Logging ──────────────────────────────────────────────────────────

    /// <summary>
    /// Configure Serilog avant la construction du DI.
    /// Fichier : %AppData%\LOLInfo\logs\lolinfo-YYYYMMDD.log
    /// Rotation quotidienne, 7 fichiers conservés.
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

    // ── DI ───────────────────────────────────────────────────────────────

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging : ILogger<T> disponible dans tous les services via injection
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // Version DataDragon — initialisée au démarrage avant toute navigation
        services.AddSingleton<IPatchVersionService, PatchVersionService>();

        // Services réseau
        services.AddSingleton<IRiotClient, RiotClient>();
        services.AddSingleton<ICdragonClient, CdragonClient>();
        services.AddSingleton<IItemClient, ItemClient>();

        // Stockage local
        services.AddSingleton<IFavoritesService, FavoritesService>();
        services.AddSingleton<ILanguageService, LanguageService>();
        services.AddSingleton<IImageCacheService, ImageCacheService>();

        // Navigation
        services.AddSingleton<IViewManager, ViewManager>();

        // ViewModels
        services.AddTransient<IAllChampionViewModel, AllChampionViewModel>();
        services.AddSingleton<IItemsViewModel, ItemsViewModel>();
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }

    // ── Cycle de vie ──────────────────────────────────────────────────────

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        this.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

        // Purge en arrière-plan les images en cache inutilisées depuis > 30 jours
        // (icônes d'anciens patchs). Non bloquant pour le démarrage.
        var imageCache = this.Services.GetRequiredService<IImageCacheService>();
        _ = Task.Run(() => imageCache.PurgeOldFiles(TimeSpan.FromDays(30)));

        // Récupère la version DataDragon courante avant la navigation.
        // En cas d'échec réseau, DataDragonCdn.Version reste "latest".
        try
        {
            await this.Services.GetRequiredService<IPatchVersionService>().InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Échec de l'initialisation de la version DataDragon — poursuite avec la version par défaut");
        }

        Log.Information("Navigation initiale vers AllChampionPage");
        this.Services.GetRequiredService<IViewManager>().NavigateToAllChampion();

        // Précharge les objets en arrière-plan pour l'onglet Objets.
        _ = this.Services.GetRequiredService<IItemsViewModel>().LoadAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("=== LOLInfo arrêt (code {ExitCode}) ===", e.ApplicationExitCode);
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    // ── Handlers exceptions non gérées ────────────────────────────────────

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Exception non gérée sur le thread UI");
    }

    private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.ExceptionObject as Exception,
            "Exception non gérée sur un thread background (IsTerminating={IsTerminating})", e.IsTerminating);
        Log.CloseAndFlush();
    }
}
