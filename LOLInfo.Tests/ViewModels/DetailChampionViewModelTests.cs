namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LOLInfo.IServices;
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
            Dictionary<string, Dictionary<string, SpellCalculation>>? cdragonCalcs = null)
        {
            var mockRiot = new Mock<IRiotClient>();
            mockRiot.Setup(r => r.GetChampionDetail(It.IsAny<string>()))
                    .ReturnsAsync(champion);

            var mockCdragon = new Mock<ICdragonClient>();
            mockCdragon.Setup(c => c.GetSpellCalculationsAsync(It.IsAny<string>()))
                       .ReturnsAsync(cdragonCalcs ?? new Dictionary<string, Dictionary<string, SpellCalculation>>());

            return new DetailChampionViewModel(
                mockRiot.Object,
                mockCdragon.Object,
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
            Assert.AreEqual("BaseDamage", qSpell.FormulaRows[0].Label);
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
