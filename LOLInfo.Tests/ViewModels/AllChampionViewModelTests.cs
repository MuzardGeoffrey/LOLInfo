namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using LOLInfo.IServices;
    using LOLInfo.IServices.Storage;
    using LOLInfo.Models;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests d'intégration pour AllChampionViewModel.
    ///
    /// GetAllChampions() déclenche CollectionViewSource qui nécessite un Dispatcher WPF.
    /// On contourne en initialisant le Dispatcher dans le constructeur de test.
    /// </summary>
    [TestClass]
    public class AllChampionViewModelTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        private static Champion MakeChampion(
            string id,
            string name,
            int attack = 3,
            int magic = 3,
            int difficulty = 5,
            double range = 300,
            string partype = "Mana",
            params string[] tags)
        {
            return new Champion
            {
                Id       = id,
                Name     = name,
                Info     = new Info { Attack = attack, Magic = magic, Difficulty = difficulty },
                Stats    = new Dictionary<string, double> { ["attackrange"] = range },
                Partype  = partype,
                Tags     = tags.Length > 0 ? new List<string>(tags) : new List<string> { "Fighter" },
            };
        }

        private static (AllChampionViewModel vm, Mock<IRiotClient> mockRiot) CreateVm(
            IEnumerable<Champion> champions)
        {
            var mockRiot = new Mock<IRiotClient>();
            mockRiot
                .Setup(r => r.GetAllChampions())
                .ReturnsAsync(new ObservableCollection<Champion>(champions));

            var mockFav = new Mock<IFavoritesService>();
            mockFav.Setup(s => s.IsFavorite(It.IsAny<string>())).Returns(false);
            mockFav.Setup(s => s.Toggle(It.IsAny<string>())).Returns(true);

            var logger = NullLogger<AllChampionViewModel>.Instance;

            var vm = new AllChampionViewModel(mockRiot.Object, mockFav.Object, logger);
            return (vm, mockRiot);
        }

        private static ObservableCollection<Champion> SampleChampions() =>
            new ObservableCollection<Champion>
            {
                MakeChampion("Ahri",    "Ahri",    attack: 3, magic: 8,  difficulty: 4, range: 550, partype: "Mana",   "Mage", "Assassin"),
                MakeChampion("Garen",   "Garen",   attack: 7, magic: 3,  difficulty: 2, range: 175, partype: "None",   "Fighter", "Tank"),
                MakeChampion("Caitlyn", "Caitlyn", attack: 9, magic: 1,  difficulty: 6, range: 650, partype: "Mana",   "Marksman"),
                MakeChampion("Lux",     "Lux",     attack: 2, magic: 9,  difficulty: 5, range: 550, partype: "Mana",   "Mage", "Support"),
                MakeChampion("Lee Sin", "Lee Sin", attack: 8, magic: 3,  difficulty: 9, range: 175, partype: "Energy", "Fighter", "Assassin"),
            };

        // ── GetAllChampions ────────────────────────────────────────────────

        [TestMethod]
        public async Task GetAllChampions_LoadsChampions_ChampionsViewNotNull()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();
            Assert.IsNotNull(vm.ChampionsView);
        }

        [TestMethod]
        public async Task GetAllChampions_BuildsTagFilters()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();
            Assert.IsTrue(vm.TagFilters.Any());
        }

        [TestMethod]
        public async Task GetAllChampions_BuildsPartypeFilters()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();
            Assert.IsTrue(vm.PartypeFilters.Any());
        }

        // ── Filtre : Nom ───────────────────────────────────────────────────

        [TestMethod]
        public async Task NameFilter_FiltersChampionsByName()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.NameFilter = "Lux";

            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Lux", visible[0].Champion.Name);
        }

        [TestMethod]
        public async Task NameFilter_CaseInsensitive()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.NameFilter = "ahri";
            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            Assert.IsTrue(visible.Any(v => v.Champion.Name == "Ahri"));
        }

        [TestMethod]
        public async Task NameFilter_Empty_ShowsAll()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.NameFilter = string.Empty;
            var count = vm.ChampionsView.Cast<ChampionListItemViewModel>().Count();
            Assert.AreEqual(5, count);
        }

        // ── Filtre : Type de dégâts ────────────────────────────────────────

        [TestMethod]
        public async Task DamageTypeFilter_AP_ShowsOnlyAP()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.SelectedDamageType = DamageTypeFilter.AP;

            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            foreach (var v in visible)
                Assert.AreEqual(DamageTypeFilter.AP, v.DamageType);
        }

        [TestMethod]
        public async Task DamageTypeFilter_AD_ShowsOnlyAD()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.SelectedDamageType = DamageTypeFilter.AD;

            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            foreach (var v in visible)
                Assert.AreEqual(DamageTypeFilter.AD, v.DamageType);
        }

        [TestMethod]
        public async Task DamageTypeFilter_Tous_ShowsAll()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.SelectedDamageType = DamageTypeFilter.Tous;
            Assert.AreEqual(5, vm.ChampionsView.Cast<object>().Count());
        }

        // ── Filtre : Portée ────────────────────────────────────────────────

        [TestMethod]
        public async Task RangeTypeFilter_Range_ShowsOnlyRanged()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.SelectedRangeType = RangeTypeFilter.Range;

            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            foreach (var v in visible)
                Assert.IsTrue(v.IsRanged);
        }

        [TestMethod]
        public async Task RangeTypeFilter_Melee_ShowsOnlyMelee()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.SelectedRangeType = RangeTypeFilter.Melee;

            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            foreach (var v in visible)
                Assert.IsFalse(v.IsRanged);
        }

        // ── Filtre : Difficulté ────────────────────────────────────────────

        [TestMethod]
        public async Task DifficultyFilter_MinMax_FiltersCorrectly()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.DifficultyMin = 5;
            vm.DifficultyMax = 7;

            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            foreach (var v in visible)
            {
                var d = v.Champion.Info?.Difficulty ?? 0;
                Assert.IsTrue(d >= 5 && d <= 7);
            }
        }

        [TestMethod]
        public void DifficultyMin_ClampedToMax()
        {
            var (vm, _) = CreateVm(Enumerable.Empty<Champion>());
            vm.DifficultyMax = 5;
            vm.DifficultyMin = 8; // doit être clampé à 5
            Assert.AreEqual(5, vm.DifficultyMin);
        }

        [TestMethod]
        public void DifficultyMax_ClampedToMin()
        {
            var (vm, _) = CreateVm(Enumerable.Empty<Champion>());
            vm.DifficultyMin = 5;
            vm.DifficultyMax = 2; // doit être clampé à 5
            Assert.AreEqual(5, vm.DifficultyMax);
        }

        // ── Filtre : Tags ──────────────────────────────────────────────────

        [TestMethod]
        public async Task TagFilter_Mage_ShowsOnlyMages()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            var mageFilter = vm.TagFilters.FirstOrDefault(f => f.Label == "Mage");
            Assert.IsNotNull(mageFilter);
            mageFilter!.IsSelected = true;

            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            foreach (var v in visible)
                Assert.IsTrue((v.Champion.Tags ?? new List<string>()).Contains("Mage"));
        }

        // ── Filtre : Favoris ───────────────────────────────────────────────

        [TestMethod]
        public async Task ShowFavoritesOnly_FiltersNonFavorites()
        {
            // Seul "Ahri" est favori
            var mockRiot = new Mock<IRiotClient>();
            mockRiot.Setup(r => r.GetAllChampions())
                .ReturnsAsync(new ObservableCollection<Champion>(SampleChampions()));

            var mockFav = new Mock<IFavoritesService>();
            mockFav.Setup(s => s.IsFavorite("Ahri")).Returns(true);
            mockFav.Setup(s => s.IsFavorite(It.Is<string>(id => id != "Ahri"))).Returns(false);

            var vm = new AllChampionViewModel(
                mockRiot.Object,
                mockFav.Object,
                NullLogger<AllChampionViewModel>.Instance);

            await vm.GetAllChampions();
            vm.ShowFavoritesOnly = true;

            var visible = vm.ChampionsView.Cast<ChampionListItemViewModel>().ToList();
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Ahri", visible[0].Champion.Id);
        }

        // ── Tri ────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task Sort_NomAZ_SortsAlphabetically()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.SelectedSortOption = vm.SortOptions.First(o => o.Key == SortOption.NomAZ);

            var names = vm.ChampionsView.Cast<ChampionListItemViewModel>()
                .Select(v => v.Champion.Name)
                .ToList();

            var sorted = names.OrderBy(n => n, System.StringComparer.OrdinalIgnoreCase).ToList();
            CollectionAssert.AreEqual(sorted, names);
        }

        [TestMethod]
        public async Task Sort_NomZA_SortsReverseAlphabetically()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.SelectedSortOption = vm.SortOptions.First(o => o.Key == SortOption.NomZA);

            var names = vm.ChampionsView.Cast<ChampionListItemViewModel>()
                .Select(v => v.Champion.Name)
                .ToList();

            var sorted = names.OrderByDescending(n => n, System.StringComparer.OrdinalIgnoreCase).ToList();
            CollectionAssert.AreEqual(sorted, names);
        }

        [TestMethod]
        public async Task Sort_DifficulteAsc_SortsByDifficultyAscending()
        {
            var (vm, _) = CreateVm(SampleChampions());
            await vm.GetAllChampions();

            vm.SelectedSortOption = vm.SortOptions.First(o => o.Key == SortOption.DifficulteAsc);

            var difficulties = vm.ChampionsView.Cast<ChampionListItemViewModel>()
                .Select(v => v.Champion.Info?.Difficulty ?? 0)
                .ToList();

            for (int i = 1; i < difficulties.Count; i++)
                Assert.IsTrue(difficulties[i] >= difficulties[i - 1]);
        }

        // ── SortOptions ────────────────────────────────────────────────────

        [TestMethod]
        public void SortOptions_Contains4Entries()
        {
            var (vm, _) = CreateVm(Enumerable.Empty<Champion>());
            Assert.AreEqual(4, vm.SortOptions.Count);
        }
    }
}
