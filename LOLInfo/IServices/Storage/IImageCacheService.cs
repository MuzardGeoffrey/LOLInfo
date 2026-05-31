namespace LOLInfo.IServices.Storage;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

/// <summary>
/// Cache disque des images du CDN : télécharge chaque image une seule fois,
/// puis la sert depuis le disque local (y compris hors-ligne et entre démarrages).
/// </summary>
public interface IImageCacheService
{
    /// <summary>
    /// Retourne l'image (gelée, utilisable depuis n'importe quel thread).
    /// Sert depuis le disque si déjà en cache, sinon télécharge puis enregistre.
    /// Retourne null en cas d'échec ou d'URL vide.
    /// </summary>
    Task<ImageSource?> GetImageAsync(string url, CancellationToken ct = default);

    /// <summary>
    /// Supprime les fichiers du cache non utilisés depuis plus de <paramref name="maxAge"/>.
    /// À appeler au démarrage pour borner la taille du dossier (icônes d'anciens patchs).
    /// </summary>
    void PurgeOldFiles(TimeSpan maxAge);
}
