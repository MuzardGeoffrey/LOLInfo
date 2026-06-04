namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LOLInfo.IServices;
    using LOLInfo.IViewModels;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DetailChampionViewModelTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        private static Champion MakeFullChampion(string id = "Ahri")
        {
            return new Champion
            {
                Id   = id,
                Name = id,
                Passive = new Passive { Name = "Essence de Renard", Description = "...", Image = new Image { Full = "AhriP.png" } },
                Spells = new List<Spell>
                {
                    new Spell { Name = "Q", Description = "Orbe", CooldownBurn = "9/8/7/6/5", CostBurn = "60", RangeBurn = "970", Image = new Image { Full = "AhriQ.png" } },
                    new Spell { Name = "W", Description = "Feux", CooldownBurn = "12",         CostBurn = "55", RangeBurn = "0",   Image = new Image { Full = "AhriW.png" } },
                    new Spell { Name = "E", Description = "Séduction", CooldownBurn = "14",    CostBurn = "70", RangeBurn = "975", Image = new Image { Full = "AhriE.png" } },
                    new Spell { Name = "R", Description = "Esprit Renard", CooldownBurn = "130/105/80", CostBurn = "0", RangeBurn = "450", Image = new Image { Full = "AhriR.png" } },
                }
            };
        }

        private static DetailChampionViewModel CreateVm(
            Champion champion,
            Dictionary<string, Dictionary<string, SpellCalculation>>? cdragonCalcs = null,
            System.Collections.Generic.IReadOnlyList<ItemViewModel>? availableItems = null)
        {
            var mockRiot = new Mock<IRiotClient>();
            mockRiot.Setup(r => r.GetChampionDetail(It.IsAny<string>()))
                    .ReturnsAsync(champion);

            var mockCdragon = new Mock<ICdragonClient>();
            mockCdragon.Setup(c => c.GetSpellCalculationsAsync(It.IsAny<string>()))
                       .ReturnsAsync(cdragonCalcs ?? new Dictionary<string, Dictionary<string, SpellCalculation>>());

            var mockItems = new Mock<IItemsViewModel>();
            mockItems.Setup(i => i.AllItems).Returns(availableItems ?? new List<ItemViewModel>());
            mockItems.Setup(i => i.SearchSuggestions).Returns(availableItems ?? new List<ItemViewModel>());

            return new DetailChampionViewModel(
                mockRiot.Object,
                mockCdragon.Object,
                mockItems.Object,
                champion.Id ?? "Ahri",
                NullLogger<DetailChampionViewModel>.Instance);
        }

        // ── Placeholders avant LoadAsync ───────────────────────────────────

        [TestMethod]
        public void Spells_BeforeLoad_HasFivePlaceholders()
        {
            var vm = CreateVm(MakeFullChampion());
            Assert.AreEqual(5, vm.Spells.Count);
        }

        [TestMethod]
        public void Champion_BeforeLoad_IsNull()
        {
            var vm = CreateVm(MakeFullChampion());
            Assert.IsNull(vm.Champion);
        }

        [TestMethod]
        public void ChampionName_IsSetFromConstructor()
        {
            var vm = CreateVm(MakeFullChampion("Lux"));
            Assert.AreEqual("Lux", vm.ChampionName);
        }

        // ── Après LoadAsync ────────────────────────────────────────────────

        [TestMethod]
        public async Task LoadAsync_SetsChampion()
        {
            var vm = CreateVm(MakeFullChampion("Ahri"));
            await vm.LoadAsync();
            Assert.IsNotNull(vm.Champion);
            Assert.AreEqual("Ahri", vm.Champion!.Name);
        }

        [TestMethod]
        public async Task LoadAsync_BuildsFiveSpells()
        {
            var vm = CreateVm(MakeFullChampion());
            await vm.LoadAsync();
            Assert.AreEqual(5, vm.Spells.Count);
        }

        [TestMethod]
        public async Task LoadAsync_FirstSpellIsPassif()
        {
            var vm = CreateVm(MakeFullChampion());
            await vm.LoadAsync();
            Assert.AreEqual("Passif", vm.Spells[0].Key);
            Assert.IsTrue(vm.Spells[0].IsPassive);
        }

        [TestMethod]
        public async Task LoadAsync_SpellKeysAreQWER()
        {
            var vm = CreateVm(MakeFullChampion());
            await vm.LoadAsync();

            Assert.AreEqual("Q", vm.Spells[1].Key);
            Assert.AreEqual("W", vm.Spells[2].Key);
            Assert.AreEqual("E", vm.Spells[3].Key);
            Assert.AreEqual("R", vm.Spells[4].Key);
        }

        [TestMethod]
        public async Task LoadAsync_RaisesPropertyChangedForChampion()
        {
            var vm = CreateVm(MakeFullChampion());
            bool raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Champion)) raised = true;
            };

            await vm.LoadAsync();
            Assert.IsTrue(raised);
        }

        [TestMethod]
        public async Task LoadAsync_RaisesPropertyChangedForSpells()
        {
            var vm = CreateVm(MakeFullChampion());
            bool raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Spells)) raised = true;
            };

            await vm.LoadAsync();
            Assert.IsTrue(raised);
        }

        // ── Enrichissement CDragon ─────────────────────────────────────────

        [TestMethod]
        public async Task LoadAsync_WithCdragonCalcs_EnrichesQSpell()
        {
            var calcs = new Dictionary<string, Dictionary<string, SpellCalculation>>
            {
                ["AhriQ"] = new Dictionary<string, SpellCalculation>
                {
                    ["BaseDamage"] = new SpellCalculation("BaseDamage",
                        new List<IFormulaPart> { new NumberPart(60) })
                }
            };

            var vm = CreateVm(MakeFullChampion("Ahri"), calcs);
            await vm.LoadAsync();

            var qSpell = vm.Spells[1]; // Q est à l'index 1
            Assert.IsTrue(qSpell.HasFormulas);
            Assert.AreEqual("Base Damage", qSpell.FormulaRows[0].Label); // nom de calcul humanisé
        }

        [TestMethod]
        public async Task LoadAsync_NoCdragonCalcs_SpellsHaveNoFormulas()
        {
            var vm = CreateVm(MakeFullChampion());
            await vm.LoadAsync();

            // Aucune formule CDragon → FormulaRows vides
            foreach (var spell in vm.Spells)
                Assert.AreEqual(0, spell.FormulaRows.Count);
        }

        [TestMethod]
        public async Task LoadAsync_CdragonKeyMismatch_SpellHasNoFormulas()
        {
            // Clé CDragon incorrecte → pas d'enrichissement
            var calcs = new Dictionary<string, Dictionary<string, SpellCalculation>>
            {
                ["WrongChampQ"] = new Dictionary<string, SpellCalculation>
                {
                    ["X"] = new SpellCalculation("X", new List<IFormulaPart> { new NumberPart(1) })
                }
            };

            var vm = CreateVm(MakeFullChampion("Ahri"), calcs);
            await vm.LoadAsync();

            Assert.AreEqual(0, vm.Spells[1].FormulaRows.Count);
        }

        // ── Champion sans passif ───────────────────────────────────────────

        [TestMethod]
        public async Task LoadAsync_NoPassive_FourSpells()
        {
            var champion = MakeFullChampion();
            champion.Passive = null;

            var vm = CreateVm(champion);
            await vm.LoadAsync();

            // Sans passif : Passif absent + Q/W/E/R = 4 sorts
            Assert.AreEqual(4, vm.Spells.Count);
            Assert.AreEqual("Q", vm.Spells[0].Key);
        }

        // ── Robustesse ─────────────────────────────────────────────────────

        [TestMethod]
        public async Task LoadAsync_ServiceThrows_DoesNotCrash()
        {
            var mockRiot = new Mock<IRiotClient>();
            mockRiot.Setup(r => r.GetChampionDetail(It.IsAny<string>()))
                    .ThrowsAsync(new System.Exception("Erreur réseau simulée"));

            var mockCdragon = new Mock<ICdragonClient>();
            mockCdragon.Setup(c => c.GetSpellCalculationsAsync(It.IsAny<string>()))
                       .ReturnsAsync(new Dictionary<string, Dictionary<string, SpellCalculation>>());

            var vm = new DetailChampionViewModel(
                mockRiot.Object,
                mockCdragon.Object,
                new Mock<IItemsViewModel>().Object,
                "Ahri",
                NullLogger<DetailChampionViewModel>.Instance);

            // Ne doit pas lever d'exception
            await vm.LoadAsync();
            Assert.IsNull(vm.Champion);
        }

        // ── Skins ──────────────────────────────────────────────────────────

        private static Champion MakeChampionWithSkins(int skinCount, string id = "Ahri")
        {
            var champion = MakeFullChampion(id);
            var skins = new List<Skin> { new() { Num = 0, Name = "default" } };
            for (int i = 1; i < skinCount; i++)
                skins.Add(new Skin { Num = i, Name = $"Skin {i}" });
            champion.Skins = skins;
            return champion;
        }

        [TestMethod]
        public async Task ChampionStats_BuiltOnLoad_AndRebuiltOnLevelChange()
        {
            var champion = MakeFullChampion();
            champion.Stats = new Dictionary<string, double>
            {
                ["hp"] = 600, ["hpperlevel"] = 100,
                ["attackspeed"] = 0.625, ["attackspeedperlevel"] = 2,
                ["movespeed"] = 340, ["attackrange"] = 175,
            };

            var vm = CreateVm(champion);
            await vm.LoadAsync();

            Assert.AreEqual(18, vm.Levels.Count);
            Assert.AreEqual(13, vm.ChampionStats.Count);

            // Première ligne = PV. Au niveau 1 = base (600).
            Assert.AreEqual("600", vm.ChampionStats[0].Value);

            // Changer de niveau recalcule : niv 18 → 600 + 100*17 = 2300.
            vm.SelectedLevel = 18;
            Assert.AreEqual("2300", vm.ChampionStats[0].Value);
        }

        [TestMethod]
        public async Task EquipItem_ModifiesChampionStats_AndUnequipReverts()
        {
            var champion = MakeFullChampion();
            champion.Stats = new Dictionary<string, double> { ["attackdamage"] = 60 };

            var infinityEdge = ItemViewModel.From(new Item
            {
                Id = "3031",
                Name = "Infinity Edge",
                Description = string.Empty,
                Stats = new Dictionary<string, double> { ["FlatPhysicalDamageMod"] = 70 },
            });

            var vm = CreateVm(champion, availableItems: new[] { infinityEdge });
            await vm.LoadAsync();

            // Index 6 = Dégâts d'attaque. Sans objet : base 60.
            Assert.AreEqual("60", vm.ChampionStats[6].Value);

            vm.ItemToEquip = infinityEdge;
            vm.EquipSelected();

            Assert.AreEqual(1, vm.EquippedItems.Count);
            Assert.AreEqual("130", vm.ChampionStats[6].Value); // 60 + 70

            vm.Unequip(infinityEdge);
            Assert.AreEqual(0, vm.EquippedItems.Count);
            Assert.AreEqual("60", vm.ChampionStats[6].Value);
        }

        [TestMethod]
        public async Task EquipSelected_ClearsSelector_AfterEquipping()
        {
            var champion = MakeFullChampion();
            champion.Stats = new Dictionary<string, double> { ["attackdamage"] = 60 };

            var ie = ItemViewModel.From(new Item
            {
                Id = "3031", Name = "Infinity Edge", Description = string.Empty,
                Stats = new Dictionary<string, double> { ["FlatPhysicalDamageMod"] = 70 },
            });

            var vm = CreateVm(champion, availableItems: new[] { ie });
            await vm.LoadAsync();

            vm.EquipQuery = "Infinity";
            vm.ItemToEquip = ie;
            vm.EquipSelected();

            Assert.AreEqual(1, vm.EquippedItems.Count);
            Assert.IsNull(vm.ItemToEquip);
            Assert.AreEqual(string.Empty, vm.EquipQuery);
        }

        [TestMethod]
        public void EquipSuggestions_ComeFromItemsViewModel()
        {
            var ie = ItemViewModel.From(new Item { Id = "3031", Name = "Infinity Edge" });
            var vm = CreateVm(MakeFullChampion(), availableItems: new[] { ie });
            Assert.AreEqual(1, vm.EquipSuggestions.Count);
        }

        [TestMethod]
        public async Task EquipItem_StopsAtSixItems()
        {
            var champion = MakeFullChampion();
            champion.Stats = new Dictionary<string, double> { ["attackdamage"] = 60 };

            ItemViewModel Boot(int i) => ItemViewModel.From(new Item
            {
                Id = i.ToString(), Name = "Item " + i, Description = string.Empty,
                Stats = new Dictionary<string, double> { ["FlatPhysicalDamageMod"] = 10 },
            });

            var vm = CreateVm(champion);
            await vm.LoadAsync();

            for (int i = 0; i < 8; i++) { vm.ItemToEquip = Boot(i); vm.EquipSelected(); }

            Assert.AreEqual(6, vm.EquippedItems.Count, "Au plus 6 objets");
            Assert.IsFalse(vm.CanEquipMore);
            Assert.AreEqual("120", vm.ChampionStats[6].Value); // 60 + 6*10
        }

        [TestMethod]
        public async Task BuildSpells_AttachesPassiveFormulas_FromCdragonPassiveKey()
        {
            var champion = MakeFullChampion("Ahri");
            var calcs = new Dictionary<string, Dictionary<string, SpellCalculation>>
            {
                ["AhriPassive"] = new()
                {
                    ["RegenCalc"] = new SpellCalculation("RegenCalc", new IFormulaPart[] { new NumberPart(100) }),
                },
            };

            var vm = CreateVm(champion, calcs);
            await vm.LoadAsync();

            var passive = vm.Spells[0];
            Assert.AreEqual("Passif", passive.Key);
            Assert.IsTrue(passive.HasFormulas, "Le passif doit afficher ses formules CDragon");
        }

        [TestMethod]
        public async Task BuildSkins_ExcludesChromaVariants()
        {
            // Reproduit le cas Ahri : un vrai skin + ses chromas (parentSkin renseigné).
            var champion = MakeFullChampion();
            champion.Skins = new List<Skin>
            {
                new() { Num = 0,  Name = "default" },
                new() { Num = 27, Name = "Spirit Blossom Ahri", Chromas = true },
                new() { Num = 29, Name = "Spirit Blossom Ahri (Aquamarine)", ParentSkin = 27 },
                new() { Num = 30, Name = "Spirit Blossom Ahri (Pearl)",      ParentSkin = 27 },
                new() { Num = 88, Name = "Spirit Blossom Springs Ahri" },
            };

            var vm = CreateVm(champion);
            await vm.LoadAsync();

            // Seuls les vrais skins sont retenus (chromas exclus), ordre préservé.
            Assert.AreEqual(3, vm.Skins.Count);
            Assert.AreEqual(0,  vm.Skins[0].Num);
            Assert.AreEqual(27, vm.Skins[1].Num);
            Assert.AreEqual(88, vm.Skins[2].Num);
        }

        [TestMethod]
        public async Task BuildSkins_MapsChromasFlag()
        {
            var champion = MakeFullChampion();
            champion.Skins = new List<Skin>
            {
                new() { Num = 0, Name = "default", Chromas = false },
                new() { Num = 1, Name = "Skin avec chromas", Chromas = true },
                new() { Num = 2, Name = "Skin sans flag", Chromas = null },
            };

            var vm = CreateVm(champion);
            await vm.LoadAsync();

            Assert.IsFalse(vm.Skins[0].HasChromas);
            Assert.IsTrue(vm.Skins[1].HasChromas);
            Assert.IsFalse(vm.Skins[2].HasChromas, "Chromas null doit être traité comme false");
        }

        [TestMethod]
        public async Task LoadAsync_SelectsFirstSkinByDefault()
        {
            var vm = CreateVm(MakeChampionWithSkins(3));
            await vm.LoadAsync();

            Assert.IsTrue(vm.HasSkins);
            Assert.AreEqual(3, vm.Skins.Count);
            Assert.IsNotNull(vm.SelectedSkin);
            Assert.AreEqual(0, vm.SelectedSkin!.Num);
        }

        [TestMethod]
        public async Task BaseSkin_DisplayName_IsChampionName()
        {
            var vm = CreateVm(MakeChampionWithSkins(2, id: "Ahri"));
            await vm.LoadAsync();

            // Le skin de base ("default") prend le nom du champion.
            Assert.AreEqual("Ahri", vm.SelectedSkin!.DisplayName);
        }

        [TestMethod]
        public async Task SkinPath_IsChampionIdUnderscoreNum()
        {
            var vm = CreateVm(MakeChampionWithSkins(3, id: "Ahri"));
            await vm.LoadAsync();

            Assert.AreEqual("Ahri_0.jpg", vm.SelectedSkin!.SkinPath);
            Assert.AreEqual("Ahri_1.jpg", vm.NextSkin!.SkinPath);
        }

        [TestMethod]
        public async Task SelectNextSkin_AdvancesAndLoopsToFirst()
        {
            var vm = CreateVm(MakeChampionWithSkins(3));
            await vm.LoadAsync();

            vm.SelectNextSkin();
            Assert.AreEqual(1, vm.SelectedSkin!.Num);

            vm.SelectNextSkin();
            Assert.AreEqual(2, vm.SelectedSkin!.Num);

            vm.SelectNextSkin(); // dernier → revient au premier (boucle)
            Assert.AreEqual(0, vm.SelectedSkin!.Num);
        }

        [TestMethod]
        public async Task SelectPreviousSkin_FromFirst_LoopsToLast()
        {
            var vm = CreateVm(MakeChampionWithSkins(3));
            await vm.LoadAsync();

            vm.SelectPreviousSkin(); // premier → revient au dernier (boucle)
            Assert.AreEqual(2, vm.SelectedSkin!.Num);
        }

        [TestMethod]
        public async Task Previews_PointToNeighbours_WithLooping()
        {
            var vm = CreateVm(MakeChampionWithSkins(3));
            await vm.LoadAsync();

            // Skin courant = 0 → précédent boucle sur le dernier (2), suivant = 1.
            Assert.AreEqual(2, vm.PreviousSkin!.Num);
            Assert.AreEqual(1, vm.NextSkin!.Num);
        }

        [TestMethod]
        public async Task Previews_AreNull_WhenSingleSkin()
        {
            var vm = CreateVm(MakeChampionWithSkins(1));
            await vm.LoadAsync();

            Assert.IsNull(vm.PreviousSkin);
            Assert.IsNull(vm.NextSkin);
        }

        [TestMethod]
        public async Task HasSkins_False_WhenNoSkins()
        {
            var champion = MakeFullChampion();
            champion.Skins = null;

            var vm = CreateVm(champion);
            await vm.LoadAsync();

            Assert.IsFalse(vm.HasSkins);
            Assert.IsNull(vm.SelectedSkin);
        }
    }
}
