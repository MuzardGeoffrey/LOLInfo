namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LOLInfo.IServices;
    using LOLInfo.Models;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;

    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests du filtre par mode de jeu et du tri (nom, coût, statistique) de ItemsViewModel.
    ///
    /// LoadAsync() construit un CollectionView qui nécessite un Dispatcher WPF,
    /// disponible sur le thread de test (cf. AllChampionViewModelTests).
    /// </summary>
    [TestClass]
    public class ItemsViewModelTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        private static Item MakeItem(
            string id,
            string name,
            int gold = 1000,
            Dictionary<string, bool>? maps = null,
            Dictionary<string, double>? stats = null,
            string? description = null) => new()
        {
            Id          = id,
            Name        = name,
            Image       = new Image { Full = id + ".png" },
            Gold        = new ItemGold { Total = gold },
            Maps        = maps,
            Stats       = stats,
            Description = description,
        };

        private static ItemsViewModel CreateVm(params Item[] items)
        {
            var mock = new Mock<IItemClient>();
            mock.Setup(c => c.GetAllItems()).ReturnsAsync(items.ToList());
            return new ItemsViewModel(mock.Object, NullLogger<ItemsViewModel>.Instance);
        }

        private static List<ItemViewModel> Visible(ItemsViewModel vm)
            => vm.ItemsView.Cast<ItemViewModel>().ToList();

        // Cartes : "11" = Faille, "12" = ARAM, "30" = Arena.
        private static Dictionary<string, bool> Maps(bool rift, bool aram, bool arena)
            => new() { ["11"] = rift, ["12"] = aram, ["30"] = arena };

        // ── Filtre : mode de jeu ───────────────────────────────────────────

        [TestMethod]
        public async Task GameMode_Tous_ShowsAllItems()
        {
            var vm = CreateVm(
                MakeItem("1", "A", maps: Maps(rift: true,  aram: false, arena: false)),
                MakeItem("2", "B", maps: Maps(rift: false, aram: true,  arena: false)),
                MakeItem("3", "C", maps: Maps(rift: false, aram: false, arena: true)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Tous;

            Assert.AreEqual(3, Visible(vm).Count);
        }

        [TestMethod]
        public async Task GameMode_NormalClasse_ShowsOnlyRiftItems()
        {
            var vm = CreateVm(
                MakeItem("1", "RiftItem",  maps: Maps(rift: true,  aram: true, arena: true)),
                MakeItem("2", "AramOnly",  maps: Maps(rift: false, aram: true, arena: true)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.NormalClasse;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("RiftItem", visible[0].Name);
        }

        [TestMethod]
        public async Task GameMode_Arena_ShowsOnlyArenaItems()
        {
            var vm = CreateVm(
                MakeItem("1", "RiftOnly", maps: Maps(rift: true, aram: true, arena: false)),
                MakeItem("2", "Arena",    maps: Maps(rift: true, aram: true, arena: true)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Arena;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Arena", visible[0].Name);
        }

        [TestMethod]
        public async Task GameMode_Aram_ShowsOnlyAramItems()
        {
            var vm = CreateVm(
                MakeItem("1", "Aram",     maps: Maps(rift: true, aram: true,  arena: true)),
                MakeItem("2", "NoAram",   maps: Maps(rift: true, aram: false, arena: true)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Aram;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Aram", visible[0].Name);
        }

        [TestMethod]
        public async Task GameMode_AndNameFilter_CombineConjunctively()
        {
            var vm = CreateVm(
                MakeItem("1", "Sword",  maps: Maps(rift: true, aram: false, arena: true)),
                MakeItem("2", "Shield", maps: Maps(rift: true, aram: false, arena: true)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Arena;
            vm.NameFilter = "swo";

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Sword", visible[0].Name);
        }

        // ── Déduplication (un objet = un nom, conscient du mode) ────────────

        [TestMethod]
        public async Task Dedup_Tous_KeepsRiftVariant_AmongModeCopies()
        {
            // Une même "Spatule" publiée en 3 variantes, une par mode.
            var vm = CreateVm(
                MakeItem("664403", "Spatule", maps: Maps(rift: true,  aram: false, arena: false)),
                MakeItem("994403", "Spatule", maps: Maps(rift: false, aram: true,  arena: false)),
                MakeItem("224403", "Spatule", maps: Maps(rift: false, aram: false, arena: true)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Tous;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("664403", visible[0].Id); // la variante Faille
        }

        [TestMethod]
        public async Task Dedup_Tous_PrefersCheapestRiftVariant()
        {
            // Deux variantes disponibles sur la Faille : on garde la moins chère.
            var vm = CreateVm(
                MakeItem("323003", "Archange", gold: 3200, maps: Maps(rift: true, aram: false, arena: false)),
                MakeItem("3003",   "Archange", gold: 2900, maps: Maps(rift: true, aram: true,  arena: false)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Tous;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("3003", visible[0].Id);
        }

        [TestMethod]
        public async Task Dedup_Arena_KeepsArenaVariant()
        {
            var vm = CreateVm(
                MakeItem("664403", "Spatule", maps: Maps(rift: true,  aram: false, arena: false)),
                MakeItem("224403", "Spatule", maps: Maps(rift: false, aram: false, arena: true)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Arena;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("224403", visible[0].Id);
        }

        [TestMethod]
        public async Task Dedup_DistinctNames_AreNotMerged()
        {
            var vm = CreateVm(
                MakeItem("1", "Épée",     maps: Maps(rift: true, aram: true, arena: true)),
                MakeItem("2", "Bouclier", maps: Maps(rift: true, aram: true, arena: true)));
            await vm.LoadAsync();

            Assert.AreEqual(2, Visible(vm).Count);
        }

        // ── Objets obsolètes (retirés de toutes les cartes) ────────────────

        [TestMethod]
        public async Task Obsolete_ItemOnNoMap_IsHidden()
        {
            var vm = CreateVm(
                MakeItem("1", "Actuel",   maps: Maps(rift: true,  aram: false, arena: false)),
                MakeItem("2", "Obsolète", maps: Maps(rift: false, aram: false, arena: false)));
            await vm.LoadAsync();

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Actuel", visible[0].Name);
        }

        [TestMethod]
        public async Task Obsolete_ItemWithNoMapData_IsKept()
        {
            // maps == null (aucune donnée) → on ne masque pas par prudence.
            var vm = CreateVm(MakeItem("1", "SansDonnée", maps: null));
            await vm.LoadAsync();

            Assert.AreEqual(1, Visible(vm).Count);
        }

        [TestMethod]
        public async Task Obsolete_Tous_KeepsLiveVariant_WhenNoRiftVariant()
        {
            // Même nom : variante Faille obsolète (aucune carte) + variante Arena vivante.
            var vm = CreateVm(
                MakeItem("6632",   "Pourfendeur", maps: Maps(rift: false, aram: false, arena: false)),
                MakeItem("446632", "Pourfendeur", maps: Maps(rift: false, aram: false, arena: true)));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Tous;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("446632", visible[0].Id); // la variante Arena, encore en jeu
        }

        // ── Tri ────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task Sort_ByAttackDamage_OrdersDescending_StatlessLast()
        {
            var vm = CreateVm(
                MakeItem("1", "SmallSword", stats: new() { ["FlatPhysicalDamageMod"] = 40 }),
                MakeItem("2", "Cloth",      stats: new() { ["FlatArmorMod"] = 30 }), // pas d'AD
                MakeItem("3", "BigSword",   stats: new() { ["FlatPhysicalDamageMod"] = 70 }));
            await vm.LoadAsync();

            vm.SelectedSortOption = vm.SortOptions.First(o => o.Key == "FlatPhysicalDamageMod");

            var names = Visible(vm).Select(i => i.Name).ToList();
            CollectionAssert.AreEqual(new[] { "BigSword", "SmallSword", "Cloth" }, names);
        }

        [TestMethod]
        public async Task Sort_CostAscending_OrdersByGold()
        {
            var vm = CreateVm(
                MakeItem("1", "Mid",   gold: 1000),
                MakeItem("2", "Cheap", gold: 500),
                MakeItem("3", "Pricey", gold: 2000));
            await vm.LoadAsync();

            vm.SelectedSortOption = vm.SortOptions.First(o => o.Key == "cost_asc");

            var golds = Visible(vm).Select(i => i.Gold).ToList();
            CollectionAssert.AreEqual(new[] { 500, 1000, 2000 }, golds);
        }

        [TestMethod]
        public async Task Sort_CostDescending_OrdersByGold()
        {
            var vm = CreateVm(
                MakeItem("1", "Mid",   gold: 1000),
                MakeItem("2", "Cheap", gold: 500),
                MakeItem("3", "Pricey", gold: 2000));
            await vm.LoadAsync();

            vm.SelectedSortOption = vm.SortOptions.First(o => o.Key == "cost_desc");

            var golds = Visible(vm).Select(i => i.Gold).ToList();
            CollectionAssert.AreEqual(new[] { 2000, 1000, 500 }, golds);
        }

        [TestMethod]
        public async Task Sort_Name_OrdersAlphabetically()
        {
            var vm = CreateVm(
                MakeItem("1", "Zeal"),
                MakeItem("2", "Amplifying Tome"),
                MakeItem("3", "Boots"));
            await vm.LoadAsync();

            vm.SelectedSortOption = vm.SortOptions.First(o => o.Key == "name");

            var names = Visible(vm).Select(i => i.Name).ToList();
            CollectionAssert.AreEqual(new[] { "Amplifying Tome", "Boots", "Zeal" }, names);
        }

        // ── Filtre : statistiques (multi-sélection, ET) ────────────────────

        [TestMethod]
        public async Task StatFilter_Single_KeepsOnlyItemsWithThatStat()
        {
            var vm = CreateVm(
                MakeItem("1", "Épée",  maps: Maps(true, true, true), stats: new() { ["FlatPhysicalDamageMod"] = 40 }),
                MakeItem("2", "Bâton", maps: Maps(true, true, true), stats: new() { ["FlatMagicDamageMod"] = 60 }));
            await vm.LoadAsync();

            vm.StatFilters.First(f => f.Key == "FlatPhysicalDamageMod").IsSelected = true;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Épée", visible[0].Name);
        }

        [TestMethod]
        public async Task StatFilter_Multiple_RequiresAllSelectedStats()
        {
            var vm = CreateVm(
                MakeItem("1", "AD seul",   maps: Maps(true, true, true), stats: new() { ["FlatPhysicalDamageMod"] = 40 }),
                MakeItem("2", "Crit seul", maps: Maps(true, true, true), stats: new() { ["FlatCritChanceMod"] = 0.25 }),
                MakeItem("3", "AD + Crit", maps: Maps(true, true, true), stats: new() { ["FlatPhysicalDamageMod"] = 40, ["FlatCritChanceMod"] = 0.25 }));
            await vm.LoadAsync();

            vm.StatFilters.First(f => f.Key == "FlatPhysicalDamageMod").IsSelected = true;
            vm.StatFilters.First(f => f.Key == "FlatCritChanceMod").IsSelected = true;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("AD + Crit", visible[0].Name);
        }

        [TestMethod]
        public async Task StatFilter_None_ShowsAll()
        {
            var vm = CreateVm(
                MakeItem("1", "Épée",  maps: Maps(true, true, true), stats: new() { ["FlatPhysicalDamageMod"] = 40 }),
                MakeItem("2", "Bâton", maps: Maps(true, true, true), stats: new() { ["FlatMagicDamageMod"] = 60 }));
            await vm.LoadAsync();

            Assert.AreEqual(2, Visible(vm).Count);
        }

        [TestMethod]
        public async Task StatFilter_CombinesWithGameMode()
        {
            var vm = CreateVm(
                MakeItem("1", "AD Arena", maps: Maps(rift: false, aram: false, arena: true),  stats: new() { ["FlatPhysicalDamageMod"] = 40 }),
                MakeItem("2", "AD Rift",  maps: Maps(rift: true,  aram: false, arena: false), stats: new() { ["FlatPhysicalDamageMod"] = 40 }));
            await vm.LoadAsync();

            vm.SelectedGameMode = GameModeFilter.Arena;
            vm.StatFilters.First(f => f.Key == "FlatPhysicalDamageMod").IsSelected = true;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("AD Arena", visible[0].Name);
        }

        [TestMethod]
        public async Task StatFilter_ManaRegen_MatchesBaseManaRegenFromDescription_FR()
        {
            // "Charme féérique" : régén. de base du mana uniquement dans le texte <stats>.
            // "Larme" : mana plat (ne doit PAS matcher le filtre régén. mana).
            var vm = CreateVm(
                MakeItem("1", "Charme", maps: Maps(true, true, true),
                    description: "<mainText><stats><attention>+50%</attention> de régénération de base du mana</stats></mainText>"),
                MakeItem("2", "Larme", maps: Maps(true, true, true), stats: new() { ["FlatMPPoolMod"] = 240 },
                    description: "<mainText><stats><attention>+240</attention> mana</stats></mainText>"));
            await vm.LoadAsync();

            vm.StatFilters.First(f => f.Key == "FlatMPRegenMod").IsSelected = true;

            var visible = Visible(vm);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Charme", visible[0].Name);
        }

        [TestMethod]
        public async Task StatFilter_ManaRegen_MatchesBaseManaRegenFromDescription_EN()
        {
            var vm = CreateVm(
                MakeItem("1", "Charm", maps: Maps(true, true, true),
                    description: "<mainText><stats><attention>50%</attention> Base Mana Regen</stats></mainText>"));
            await vm.LoadAsync();

            vm.StatFilters.First(f => f.Key == "FlatMPRegenMod").IsSelected = true;

            Assert.AreEqual(1, Visible(vm).Count);
        }

        [TestMethod]
        public void StatFilters_ExposeAllSortableStats()
        {
            var vm = CreateVm();
            Assert.AreEqual(11, vm.StatFilters.Count);
            Assert.IsTrue(vm.StatFilters.Any(f => f.Key == "FlatPhysicalDamageMod"));
        }

        // ── SortOptions ────────────────────────────────────────────────────

        [TestMethod]
        public void SortOptions_DefaultsToName()
        {
            var vm = CreateVm();
            Assert.AreEqual("name", vm.SelectedSortOption.Key);
        }

        [TestMethod]
        public void SortOptions_ContainsNameCostAndStatEntries()
        {
            var vm = CreateVm();

            Assert.IsTrue(vm.SortOptions.Any(o => o.Key == "name"));
            Assert.IsTrue(vm.SortOptions.Any(o => o.Key == "cost_asc"));
            Assert.IsTrue(vm.SortOptions.Any(o => o.Key == "cost_desc"));
            Assert.IsTrue(vm.SortOptions.Any(o => o.Key == "FlatPhysicalDamageMod"));
            Assert.IsTrue(vm.SortOptions.Any(o => o.Key == "FlatMagicDamageMod"));
        }
    }
}
