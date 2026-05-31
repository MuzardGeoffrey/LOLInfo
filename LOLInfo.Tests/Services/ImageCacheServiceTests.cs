namespace LOLInfo.Tests.Services;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using LOLInfo.Services.Storage;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests pour ImageCacheService.
///
/// Le service écrit dans %AppData%\LOLInfo\cache\images. Comme les autres
/// services de stockage, on redirige le dossier via réflexion vers un dossier
/// temporaire pour ne pas polluer l'environnement ni dépendre du réseau.
/// </summary>
[TestClass]
public class ImageCacheServiceTests
{
    private string _tempDir = string.Empty;
    private string _originalDir = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"lolinfo_imgcache_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        var field = typeof(ImageCacheService)
            .GetField("CacheDirectory", BindingFlags.Static | BindingFlags.NonPublic);
        _originalDir = (string)(field?.GetValue(null) ?? string.Empty);
        field?.SetValue(null, _tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        var field = typeof(ImageCacheService)
            .GetField("CacheDirectory", BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, _originalDir);

        try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }
        catch { /* best effort */ }
    }

    private static ImageCacheService Create() => new(NullLogger<ImageCacheService>.Instance);

    // 1×1 PNG transparent — image valide minimale pour tester le décodage.
    private static byte[] TinyPng() => Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    [TestMethod]
    public void GetCacheFilePath_IsDeterministic_AndInsideCacheDir()
    {
        const string url = "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Ahri_0.jpg";

        var p1 = ImageCacheService.GetCacheFilePath(url);
        var p2 = ImageCacheService.GetCacheFilePath(url);

        Assert.AreEqual(p1, p2);
        StringAssert.StartsWith(p1, _tempDir);
        StringAssert.EndsWith(p1, ".jpg");
    }

    [TestMethod]
    public void GetCacheFilePath_DifferentUrls_GiveDifferentFiles()
    {
        var a = ImageCacheService.GetCacheFilePath("https://x/14.11.1/img/champion/Ahri.png");
        var b = ImageCacheService.GetCacheFilePath("https://x/14.12.1/img/champion/Ahri.png");
        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public async Task GetImageAsync_CacheHit_ReturnsImage_WithoutNetwork()
    {
        // URL volontairement injoignable : si le réseau était sollicité (miss),
        // l'appel échouerait → null. Le fichier présent force un cache hit.
        const string url = "https://example.invalid/champion/Ahri.png";
        File.WriteAllBytes(ImageCacheService.GetCacheFilePath(url), TinyPng());

        var image = await Create().GetImageAsync(url);

        Assert.IsNotNull(image);
        Assert.IsTrue(image!.IsFrozen, "L'image doit être gelée pour un usage cross-thread");
    }

    [TestMethod]
    public async Task GetImageAsync_EmptyUrl_ReturnsNull()
    {
        Assert.IsNull(await Create().GetImageAsync(""));
    }

    [TestMethod]
    public async Task GetImageAsync_CacheHit_RefreshesLastWriteTime()
    {
        const string url = "https://example.invalid/champion/Lux.png";
        var path = ImageCacheService.GetCacheFilePath(url);
        File.WriteAllBytes(path, TinyPng());
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-10));

        await Create().GetImageAsync(url);

        // La lecture « touche » le fichier → protégé de la purge par ancienneté.
        Assert.IsTrue(File.GetLastWriteTimeUtc(path) > DateTime.UtcNow.AddMinutes(-1));
    }

    [TestMethod]
    public void PurgeOldFiles_RemovesOld_KeepsRecent()
    {
        var oldFile = Path.Combine(_tempDir, "old.img");
        var newFile = Path.Combine(_tempDir, "new.img");
        File.WriteAllBytes(oldFile, [1]);
        File.WriteAllBytes(newFile, [1]);
        File.SetLastWriteTimeUtc(oldFile, DateTime.UtcNow.AddDays(-40));

        Create().PurgeOldFiles(TimeSpan.FromDays(30));

        Assert.IsFalse(File.Exists(oldFile), "Le fichier ancien doit être supprimé");
        Assert.IsTrue(File.Exists(newFile), "Le fichier récent doit être conservé");
    }

    [TestMethod]
    public void PurgeOldFiles_NoCacheDir_DoesNotThrow()
    {
        Directory.Delete(_tempDir, recursive: true);
        Create().PurgeOldFiles(TimeSpan.FromDays(30)); // ne doit pas lever
    }
}
