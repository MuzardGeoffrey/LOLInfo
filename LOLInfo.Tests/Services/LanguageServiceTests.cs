namespace LOLInfo.Tests.Services;

using System;
using System.IO;
using System.Reflection;

using LOLInfo.Localization;
using LOLInfo.Services.Storage;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests pour LanguageService.
///
/// LanguageService écrit dans %AppData%\LOLInfo\settings.json.
/// Comme FavoritesServiceTests, on redirige le chemin via réflexion vers un
/// fichier temporaire pour ne pas polluer l'environnement.
/// </summary>
[TestClass]
public class LanguageServiceTests
{
    private string _tempFile = string.Empty;
    private string _originalFilePath = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"lolinfo_lang_{Guid.NewGuid()}.json");

        var field = typeof(LanguageService)
            .GetField("FilePath", BindingFlags.Static | BindingFlags.NonPublic);

        _originalFilePath = (string)(field?.GetValue(null) ?? string.Empty);
        field?.SetValue(null, _tempFile);
    }

    [TestCleanup]
    public void Cleanup()
    {
        var field = typeof(LanguageService)
            .GetField("FilePath", BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, _originalFilePath);

        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    private static LanguageService Create() => new(NullLogger<LanguageService>.Instance);

    [TestMethod]
    public void FirstLaunch_NoFile_DefaultsToEnglishAndPersists()
    {
        var service = Create();

        Assert.AreEqual(AppLocalization.DefaultCulture, service.CurrentLanguage);
        Assert.AreEqual("en", service.CurrentLanguage);
        Assert.IsTrue(File.Exists(_tempFile), "Le défaut doit être persisté au premier lancement");
    }

    [TestMethod]
    public void SetLanguage_PersistsAcrossRestart()
    {
        Create().SetLanguage("fr");

        // Nouvelle instance = simulation d'un redémarrage : la langue est rechargée.
        var afterRestart = Create();
        Assert.AreEqual("fr", afterRestart.CurrentLanguage);
    }

    [TestMethod]
    public void SetLanguage_Unsupported_IsIgnored()
    {
        var service = Create();
        service.SetLanguage("xx");

        Assert.AreEqual("en", service.CurrentLanguage);
    }

    [TestMethod]
    public void Load_CorruptFile_FallsBackToDefault()
    {
        File.WriteAllText(_tempFile, "{ ceci n'est pas du JSON valide");

        var service = Create();
        Assert.AreEqual(AppLocalization.DefaultCulture, service.CurrentLanguage);
    }

    [TestMethod]
    public void AvailableLanguages_ContainsEnAndFr()
    {
        CollectionAssert.Contains((System.Collections.ICollection)Create().AvailableLanguages, "en");
        CollectionAssert.Contains((System.Collections.ICollection)Create().AvailableLanguages, "fr");
    }
}
