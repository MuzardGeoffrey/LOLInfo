namespace LOLInfo.Tests.Services
{
    using System;
    using System.IO;
    using System.Reflection;
    using LOLInfo.IServices.Storage;
    using LOLInfo.Services.Storage;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests pour FavoritesService.
    ///
    /// FavoritesService écrit dans %AppData%\LOLInfo\favorites.json.
    /// Pour isoler les tests, on redirige le chemin via réflexion
    /// afin d'utiliser un fichier temporaire et ne pas polluer l'environnement.
    /// </summary>
    [TestClass]
    public class FavoritesServiceTests
    {
        private string _tempFile = string.Empty;
        private string _originalFilePath = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            // Crée un fichier temporaire unique par test
            _tempFile = Path.Combine(Path.GetTempPath(), $"lolinfo_test_{Guid.NewGuid()}.json");

            // Redirige FavoritesService.FilePath vers notre fichier temporaire
            var field = typeof(FavoritesService)
                .GetField("FilePath", BindingFlags.Static | BindingFlags.NonPublic);

            _originalFilePath = (string)(field?.GetValue(null) ?? string.Empty);
            field?.SetValue(null, _tempFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Nettoyage : supprime le fichier temporaire
            if (File.Exists(_tempFile))
                File.Delete(_tempFile);

            // Restaure le chemin d'origine
            var field = typeof(FavoritesService)
                .GetField("FilePath", BindingFlags.Static | BindingFlags.NonPublic);
            field?.SetValue(null, _originalFilePath);
        }

        private FavoritesService CreateService()
            => new FavoritesService(NullLogger<FavoritesService>.Instance);

        // ── IsFavorite ────────────────────────────────────────────────────

        [TestMethod]
        public void IsFavorite_NewService_ReturnsFalse()
        {
            var svc = CreateService();
            Assert.IsFalse(svc.IsFavorite("Ahri"));
        }

        [TestMethod]
        public void IsFavorite_NullOrEmpty_ReturnsFalse()
        {
            var svc = CreateService();
            Assert.IsFalse(svc.IsFavorite(null!));
            Assert.IsFalse(svc.IsFavorite(string.Empty));
        }

        // ── Toggle ────────────────────────────────────────────────────────

        [TestMethod]
        public void Toggle_AddsFavorite_ReturnsTrue()
        {
            var svc = CreateService();
            var result = svc.Toggle("Ahri");
            Assert.IsTrue(result);
            Assert.IsTrue(svc.IsFavorite("Ahri"));
        }

        [TestMethod]
        public void Toggle_RemovesFavorite_ReturnsFalse()
        {
            var svc = CreateService();
            svc.Toggle("Ahri");       // ajoute
            var result = svc.Toggle("Ahri"); // retire
            Assert.IsFalse(result);
            Assert.IsFalse(svc.IsFavorite("Ahri"));
        }

        [TestMethod]
        public void Toggle_EmptyId_ReturnsFalse()
        {
            var svc = CreateService();
            var result = svc.Toggle(string.Empty);
            Assert.IsFalse(result);
        }

        // ── GetAll ────────────────────────────────────────────────────────

        [TestMethod]
        public void GetAll_Empty_ReturnsEmpty()
        {
            var svc = CreateService();
            Assert.AreEqual(0, svc.GetAll().Count);
        }

        [TestMethod]
        public void GetAll_AfterToggle_ContainsChampion()
        {
            var svc = CreateService();
            svc.Toggle("Lux");
            Assert.IsTrue(svc.GetAll().Contains("Lux"));
        }

        // ── Persistance ───────────────────────────────────────────────────

        [TestMethod]
        public void Favorites_PersistedAcrossInstances()
        {
            // Première instance : ajoute un favori
            {
                var svc = CreateService();
                svc.Toggle("Jinx");
            }

            // Deuxième instance : charge depuis le même fichier
            {
                var svc = CreateService();
                Assert.IsTrue(svc.IsFavorite("Jinx"));
            }
        }

        [TestMethod]
        public void Load_CorruptedFile_StartsEmpty()
        {
            // Écrit un JSON invalide
            File.WriteAllText(_tempFile, "{ invalid json !!!");
            var svc = CreateService();
            Assert.AreEqual(0, svc.GetAll().Count);
        }

        [TestMethod]
        public void MultipleFavorites_AllPersistedAndLoaded()
        {
            var ids = new[] { "Ahri", "Lux", "Thresh" };

            {
                var svc = CreateService();
                foreach (var id in ids) svc.Toggle(id);
            }

            {
                var svc = CreateService();
                foreach (var id in ids)
                    Assert.IsTrue(svc.IsFavorite(id), $"{id} devrait être en favori");
            }
        }
    }
}
