namespace LOLInfo.Services.Storage;

using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using LOLInfo.IServices.Storage;

using Microsoft.Extensions.Logging;

/// <summary>
/// Cache disque des images, dans %AppData%\LOLInfo\cache\images.
/// Chaque image est nommée d'après le hash SHA-256 de son URL — deux URLs
/// différentes (ex : icônes de patchs différents) donnent des fichiers distincts.
///
/// La purge par ancienneté s'appuie sur la date de dernière écriture, « touchée »
/// à chaque lecture (cache hit) pour refléter l'usage réel.
/// </summary>
public class ImageCacheService(ILogger<ImageCacheService> logger) : IImageCacheService
{
    // Non-readonly : redirigé par les tests via réflexion vers un dossier temporaire.
    private static string CacheDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LOLInfo", "cache", "images");

    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public async Task<ImageSource?> GetImageAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            var path = GetCacheFilePath(url);
            byte[] bytes;

            if (File.Exists(path))
            {
                bytes = await File.ReadAllBytesAsync(path, ct).ConfigureAwait(false);
                TryTouch(path);
                logger.LogDebug("[ImageCache] HIT {Url}", url);
            }
            else
            {
                logger.LogDebug("[ImageCache] MISS {Url} — téléchargement", url);
                bytes = await _http.GetByteArrayAsync(url, ct).ConfigureAwait(false);
                await SaveAtomicAsync(path, bytes, ct).ConfigureAwait(false);
            }

            return Decode(bytes);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[ImageCache] Échec pour {Url}", url);
            return null;
        }
    }

    public void PurgeOldFiles(TimeSpan maxAge)
    {
        try
        {
            if (!Directory.Exists(CacheDirectory)) return;

            var cutoff = DateTime.UtcNow - maxAge;
            int removed = 0;

            foreach (var file in Directory.EnumerateFiles(CacheDirectory))
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(file) < cutoff)
                    {
                        File.Delete(file);
                        removed++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "[ImageCache] Fichier non supprimable : {File}", file);
                }
            }

            logger.LogInformation(
                "[ImageCache] Purge : {Removed} fichier(s) supprimé(s) (inutilisés depuis > {Days} j)",
                removed, maxAge.TotalDays);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[ImageCache] Purge impossible");
        }
    }

    /// <summary>Chemin local du fichier de cache pour une URL donnée.</summary>
    public static string GetCacheFilePath(string url)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)));

        // Conserve l'extension d'origine (.png/.jpg) pour le débogage ; sinon .img.
        var ext = Path.GetExtension(url);
        if (string.IsNullOrEmpty(ext) || ext.Length > 5) ext = ".img";

        return Path.Combine(CacheDirectory, hash + ext);
    }

    private static async Task SaveAtomicAsync(string path, byte[] bytes, CancellationToken ct)
    {
        Directory.CreateDirectory(CacheDirectory);

        // Écrit dans un fichier temporaire puis renomme : évite un fichier
        // partiel si l'application est fermée pendant le téléchargement.
        var tmp = path + ".tmp";
        await File.WriteAllBytesAsync(tmp, bytes, ct).ConfigureAwait(false);
        File.Move(tmp, path, overwrite: true);
    }

    private static void TryTouch(string path)
    {
        try { File.SetLastWriteTimeUtc(path, DateTime.UtcNow); }
        catch { /* non critique */ }
    }

    private static ImageSource Decode(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption   = BitmapCacheOption.OnLoad;   // décode immédiatement, libère le flux
        bmp.StreamSource  = ms;
        bmp.CreateOptions = BitmapCreateOptions.None;
        bmp.EndInit();
        bmp.Freeze();                                   // utilisable cross-thread
        return bmp;
    }
}
