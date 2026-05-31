namespace LOLInfo.Services.Storage;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using LOLInfo.IServices.Storage;
using LOLInfo.Localization;

using Microsoft.Extensions.Logging;

/// <summary>
/// Persiste la langue choisie dans un fichier JSON local.
/// Chemin : %AppData%\LOLInfo\settings.json — ex : <c>{ "language": "en" }</c>.
///
/// Au premier lancement (fichier absent), retombe sur
/// <see cref="AppLocalization.DefaultCulture"/> (anglais) et l'enregistre.
/// La culture n'est PAS appliquée ici : c'est la racine de composition (App)
/// qui lit <see cref="CurrentLanguage"/> et appelle <see cref="AppLocalization.ApplyCulture"/>.
/// </summary>
public class LanguageService : ILanguageService
{
    // Non-readonly : redirigé par les tests via réflexion vers un fichier temporaire.
    private static string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LOLInfo",
        "settings.json");

    private readonly ILogger<LanguageService> _logger;

    public LanguageService(ILogger<LanguageService> logger)
    {
        this._logger = logger;
        this.CurrentLanguage = this.LoadOrInitialize();
    }

    public string CurrentLanguage { get; private set; }

    public IReadOnlyList<string> AvailableLanguages => AppLocalization.SupportedCultures;

    public void SetLanguage(string cultureName)
    {
        if (!AppLocalization.IsSupported(cultureName))
        {
            this._logger.LogWarning("Langue non supportée ignorée : '{Culture}'", cultureName);
            return;
        }

        if (cultureName == this.CurrentLanguage) return;

        this.CurrentLanguage = cultureName;
        this.Save();
        this._logger.LogInformation("Langue enregistrée : '{Culture}' (effective au prochain démarrage)", cultureName);
    }

    // ── Persistance ───────────────────────────────────────────────────────

    private string LoadOrInitialize()
    {
        this._logger.LogDebug("Chargement de la langue depuis {FilePath}", FilePath);
        try
        {
            if (File.Exists(FilePath))
            {
                var json     = File.ReadAllText(FilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                var saved    = settings?.Language;

                if (AppLocalization.IsSupported(saved))
                {
                    this._logger.LogInformation("Langue chargée : '{Culture}'", saved);
                    return saved!;
                }

                this._logger.LogWarning(
                    "Langue enregistrée absente ou non supportée ('{Culture}') — retour au défaut '{Default}'",
                    saved, AppLocalization.DefaultCulture);
            }
            else
            {
                this._logger.LogInformation(
                    "Aucune préférence de langue ({FilePath}) — initialisation au défaut '{Default}'",
                    FilePath, AppLocalization.DefaultCulture);
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex,
                "Impossible de lire la langue ({FilePath}) — retour au défaut '{Default}'",
                FilePath, AppLocalization.DefaultCulture);
        }

        // Persiste le défaut pour que le fichier existe dès le premier lancement.
        this.CurrentLanguage = AppLocalization.DefaultCulture;
        this.Save();
        return AppLocalization.DefaultCulture;
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var json = JsonSerializer.Serialize(
                new AppSettings { Language = this.CurrentLanguage },
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
            this._logger.LogDebug("Langue '{Culture}' sauvegardée dans {FilePath}", this.CurrentLanguage, FilePath);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Impossible de sauvegarder la langue dans {FilePath}", FilePath);
        }
    }

    /// <summary>Schéma du fichier settings.json (extensible pour d'autres préférences).</summary>
    private sealed class AppSettings
    {
        public string? Language { get; set; }
    }
}
