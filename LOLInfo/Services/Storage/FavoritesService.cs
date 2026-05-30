namespace LOLInfo.Services.Storage;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using LOLInfo.IServices.Storage;

using Microsoft.Extensions.Logging;

/// <summary>
/// Persiste la liste des champions favoris dans un fichier JSON local.
/// Chemin : %AppData%\LOLInfo\favorites.json
/// </summary>
public class FavoritesService(ILogger<FavoritesService> logger) : IFavoritesService
{
    // Non-readonly : redirigé par les tests via réflexion vers un fichier temporaire.
    private static string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LOLInfo",
        "favorites.json");

    private readonly HashSet<string> _favorites = LoadFavorites(logger);

    // ── Lecture ───────────────────────────────────────────────────────────

    public bool IsFavorite(string? championId) =>
        !string.IsNullOrEmpty(championId) && this._favorites.Contains(championId);

    public IReadOnlyCollection<string> GetAll() => this._favorites;

    // ── Écriture ──────────────────────────────────────────────────────────

    public bool Toggle(string championId)
    {
        if (string.IsNullOrEmpty(championId))
        {
            logger.LogWarning("Toggle appelé avec un championId null ou vide");
            return false;
        }

        bool isFavorite;
        if (this._favorites.Contains(championId))
        {
            this._favorites.Remove(championId);
            isFavorite = false;
            logger.LogInformation("Champion '{ChampionId}' retiré des favoris", championId);
        }
        else
        {
            this._favorites.Add(championId);
            isFavorite = true;
            logger.LogInformation("Champion '{ChampionId}' ajouté aux favoris", championId);
        }

        this.Save();
        return isFavorite;
    }

    // ── Persistance ───────────────────────────────────────────────────────

    private static HashSet<string> LoadFavorites(ILogger logger)
    {
        logger.LogDebug("Chargement des favoris depuis {FilePath}", FilePath);
        try
        {
            if (!File.Exists(FilePath))
            {
                logger.LogInformation("Fichier favoris absent, démarrage avec une liste vide ({FilePath})", FilePath);
                return [];
            }
            var json      = File.ReadAllText(FilePath);
            var favorites = JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
            logger.LogInformation("{Count} favori(s) chargé(s) depuis {FilePath}", favorites.Count, FilePath);
            return favorites;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Fichier favoris corrompu ({FilePath}) — démarrage avec une liste vide", FilePath);
            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Impossible de lire le fichier favoris ({FilePath})", FilePath);
            return [];
        }
    }

    private void Save()
    {
        logger.LogDebug("Sauvegarde de {Count} favori(s) dans {FilePath}", this._favorites.Count, FilePath);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this._favorites));
            logger.LogDebug("Favoris sauvegardés avec succès");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Impossible de sauvegarder les favoris dans {FilePath}", FilePath);
        }
    }
}
