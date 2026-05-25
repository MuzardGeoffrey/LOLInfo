namespace LOLInfo.Services.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Persiste la liste des champions favoris dans un fichier JSON local.
    /// Chemin : %AppData%\LOLInfo\favorites.json
    /// </summary>
    public class FavoritesService : IFavoritesService
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LOLInfo",
            "favorites.json");

        private readonly ILogger<FavoritesService> _logger;
        private readonly HashSet<string> _favorites;

        public FavoritesService(ILogger<FavoritesService> logger)
        {
            _logger = logger;
            _favorites = Load();
        }

        // ── Lecture ───────────────────────────────────────────────────────

        public bool IsFavorite(string championId)
            => !string.IsNullOrEmpty(championId) && _favorites.Contains(championId);

        public IReadOnlyCollection<string> GetAll() => _favorites;

        // ── Écriture ──────────────────────────────────────────────────────

        public bool Toggle(string championId)
        {
            if (string.IsNullOrEmpty(championId))
            {
                _logger.LogWarning("Toggle appelé avec un championId null ou vide");
                return false;
            }

            bool isFavorite;
            if (_favorites.Contains(championId))
            {
                _favorites.Remove(championId);
                isFavorite = false;
                _logger.LogInformation("Champion '{ChampionId}' retiré des favoris", championId);
            }
            else
            {
                _favorites.Add(championId);
                isFavorite = true;
                _logger.LogInformation("Champion '{ChampionId}' ajouté aux favoris", championId);
            }

            Save();
            return isFavorite;
        }

        // ── Persistance ───────────────────────────────────────────────────

        private HashSet<string> Load()
        {
            _logger.LogDebug("Chargement des favoris depuis {FilePath}", FilePath);

            try
            {
                if (!File.Exists(FilePath))
                {
                    _logger.LogInformation("Fichier favoris absent, démarrage avec une liste vide ({FilePath})", FilePath);
                    return new HashSet<string>();
                }

                var json = File.ReadAllText(FilePath);
                var favorites = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
                _logger.LogInformation("{Count} favori(s) chargé(s) depuis {FilePath}", favorites.Count, FilePath);
                return favorites;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Fichier favoris corrompu ({FilePath}) — démarrage avec une liste vide", FilePath);
                return new HashSet<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Impossible de lire le fichier favoris ({FilePath}) — démarrage avec une liste vide", FilePath);
                return new HashSet<string>();
            }
        }

        private void Save()
        {
            _logger.LogDebug("Sauvegarde de {Count} favori(s) dans {FilePath}", _favorites.Count, FilePath);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_favorites));
                _logger.LogDebug("Favoris sauvegardés avec succès");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Impossible de sauvegarder les favoris dans {FilePath} — les modifications seront perdues à la prochaine session", FilePath);
            }
        }
    }
}
