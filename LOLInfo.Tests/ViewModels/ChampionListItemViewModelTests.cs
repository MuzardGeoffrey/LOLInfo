namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using LOLInfo.IServices.Storage;
    using LOLInfo.Models;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ChampionListItemViewModelTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        private static Mock<IFavoritesService> MockFavorites(bool isFavorite = false)
        {
            var mock = new Mock<IFavoritesService>();
            mock.Setup(s => s.IsFavorite(It.IsAny<string>())).Returns(isFavorite);
            mock.Setup(s => s.Toggle(It.IsAny<string>())).Returns(!isFavorite);
            return mock;
        }

        private static Champion MakeChampion(
            string id = "Ahri",
            int attack = 3,
            int magic = 8,
            double attackRange = 550,
            string partype = "Mana",
            int difficulty = 5)
        {
            return new Champion
            {
                Id = id,
                Name = id,
                Info = new Info { Attack = attack, Magic = magic, Difficulty = difficulty },
                Partype = partype,
                Stats = new Dictionary<string, double> { ["attackrange"] = attackRange },
                Tags = new List<string> { "Mage" },
            };
        }

        // ── DamageType ────────────────────────────────────────────────────

        [TestMethod]
        public void DamageType_HighMagic_IsAP()
        {
            var vm = new ChampionListItemViewModel(MakeChampion(magic: 9, attack: 2), MockFavorites().Object);
            Assert.AreEqual(DamageTypeFilter.AP, vm.DamageType);
        }

        [TestMethod]
        public void DamageType_HighAttack_IsAD()
        {
            var vm = new ChampionListItemViewModel(MakeChampion(attack: 9, magic: 1), MockFavorites().Object);
            Assert.AreEqual(DamageTypeFilter.AD, vm.DamageType);
        }

        [TestMethod]
        public void DamageType_Similar_IsMixte()
        {
            // Écart < 3 → Mixte (ex: Jax 7/4)
            var vm = new ChampionListItemViewModel(MakeChampion(attack: 7, magic: 5), MockFavorites().Object);
            Assert.AreEqual(DamageTypeFilter.Mixte, vm.DamageType);
        }

        [TestMethod]
        public void DamageType_ExactlyThreeDifference_IsAD()
        {
            // Écart = 3 en faveur AD → AD
            var vm = new ChampionListItemViewModel(MakeChampion(attack: 7, magic: 4), MockFavorites().Object);
            Assert.AreEqual(DamageTypeFilter.AD, vm.DamageType);
        }

        // ── IsRanged ──────────────────────────────────────────────────────

        [TestMethod]
        public void IsRanged_AttackRange650_IsTrue()
        {
            var vm = new ChampionListItemViewModel(MakeChampion(attackRange: 650), MockFavorites().Object);
            Assert.IsTrue(vm.IsRanged);
        }

        [TestMethod]
        public void IsRanged_AttackRange175_IsFalse()
        {
            var vm = new ChampionListItemViewModel(MakeChampion(attackRange: 175), MockFavorites().Object);
            Assert.IsFalse(vm.IsRanged);
        }

        [TestMethod]
        public void IsRanged_ExactlyRange300_IsTrue()
        {
            var vm = new ChampionListItemViewModel(MakeChampion(attackRange: 300), MockFavorites().Object);
            Assert.IsTrue(vm.IsRanged);
        }

        [TestMethod]
        public void IsRanged_NoStats_IsFalse()
        {
            var champion = new Champion { Id = "X", Stats = null };
            var vm = new ChampionListItemViewModel(champion, MockFavorites().Object);
            Assert.IsFalse(vm.IsRanged);
        }

        // ── IsFavorite / ToggleFavorite ────────────────────────────────────

        [TestMethod]
        public void IsFavorite_InitiallyFalse_WhenServiceReturnsFalse()
        {
            var vm = new ChampionListItemViewModel(MakeChampion(), MockFavorites(false).Object);
            Assert.IsFalse(vm.IsFavorite);
        }

        [TestMethod]
        public void IsFavorite_InitiallyTrue_WhenServiceReturnsTrue()
        {
            var vm = new ChampionListItemViewModel(MakeChampion(), MockFavorites(true).Object);
            Assert.IsTrue(vm.IsFavorite);
        }

        [TestMethod]
        public void ToggleFavoriteCommand_CallsToggle_AndUpdatesIsFavorite()
        {
            var mockSvc = MockFavorites(false); // toggle retourne true
            var vm = new ChampionListItemViewModel(MakeChampion(id: "Ahri"), mockSvc.Object);

            vm.ToggleFavoriteCommand.Execute(null);

            mockSvc.Verify(s => s.Toggle("Ahri"), Times.Once);
            Assert.IsTrue(vm.IsFavorite);
        }

        [TestMethod]
        public void ToggleFavoriteCommand_TogglesBack_WhenCalledTwice()
        {
            int callCount = 0;
            var mock = new Mock<IFavoritesService>();
            mock.Setup(s => s.IsFavorite(It.IsAny<string>())).Returns(false);
            mock.Setup(s => s.Toggle(It.IsAny<string>())).Returns(() => ++callCount % 2 == 1);

            var vm = new ChampionListItemViewModel(MakeChampion(), mock.Object);

            vm.ToggleFavoriteCommand.Execute(null); // → true
            Assert.IsTrue(vm.IsFavorite);

            vm.ToggleFavoriteCommand.Execute(null); // → false
            Assert.IsFalse(vm.IsFavorite);
        }

        // ── PropertyChanged ────────────────────────────────────────────────

        [TestMethod]
        public void ToggleFavorite_RaisesPropertyChangedForIsFavorite()
        {
            var mock = MockFavorites(false);
            var vm = new ChampionListItemViewModel(MakeChampion(), mock.Object);

            bool raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.IsFavorite)) raised = true;
            };

            vm.ToggleFavoriteCommand.Execute(null);
            Assert.IsTrue(raised);
        }
    }
}
