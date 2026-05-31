namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SpellViewModelTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        private static Spell MakeSpell(
            string name = "Orbe",
            string cooldown = "9/8/7/6/5",
            string cost = "55/65/75/85/95",
            string range = "970",
            List<string>? leveltipLabels = null,
            List<string>? leveltipEffects = null)
        {
            return new Spell
            {
                Name         = name,
                Description  = "Inflige des dégâts",
                CooldownBurn = cooldown,
                CostBurn     = cost,
                RangeBurn    = range,
                Image        = new Image { Full = "AhriQ.png" },
                Leveltip     = (leveltipLabels != null && leveltipEffects != null)
                    ? new Leveltip { Label = leveltipLabels, Effect = leveltipEffects }
                    : null,
            };
        }

        private static Passive MakePassive(string name = "Essence de Renard") =>
            new Passive
            {
                Name        = name,
                Description = "Récupère de l'énergie",
                Image       = new Image { Full = "AhriPassive.png" },
            };

        // ── Empty ──────────────────────────────────────────────────────────

        [TestMethod]
        public void Empty_KeyIsSet()
        {
            var vm = SpellViewModel.Empty("Q");
            Assert.AreEqual("Q", vm.Key);
        }

        [TestMethod]
        public void Empty_AllFieldsAreEmpty()
        {
            var vm = SpellViewModel.Empty("W");
            Assert.AreEqual(string.Empty, vm.Name);
            Assert.AreEqual(string.Empty, vm.Description);
            Assert.AreEqual(string.Empty, vm.IconPath);
            Assert.AreEqual(0, vm.StatRows.Count);
            Assert.AreEqual(0, vm.LeveltipRows.Count);
            Assert.AreEqual(0, vm.FormulaRows.Count);
        }

        [TestMethod]
        public void Empty_HasStats_IsFalse()
        {
            Assert.IsFalse(SpellViewModel.Empty("Q").HasStats);
        }

        [TestMethod]
        public void Empty_HasLeveltip_IsFalse()
        {
            Assert.IsFalse(SpellViewModel.Empty("Q").HasLeveltip);
        }

        [TestMethod]
        public void Empty_HasFormulas_IsFalse()
        {
            Assert.IsFalse(SpellViewModel.Empty("Q").HasFormulas);
        }

        // ── FromPassive ────────────────────────────────────────────────────

        [TestMethod]
        public void FromPassive_KeyIsPassif()
        {
            var vm = SpellViewModel.FromPassive(MakePassive());
            Assert.AreEqual("Passif", vm.Key);
        }

        [TestMethod]
        public void FromPassive_IsPassiveTrue()
        {
            var vm = SpellViewModel.FromPassive(MakePassive());
            Assert.IsTrue(vm.IsPassive);
        }

        [TestMethod]
        public void FromPassive_NameAndDescriptionSet()
        {
            var vm = SpellViewModel.FromPassive(MakePassive("Essence de Renard"));
            Assert.AreEqual("Essence de Renard", vm.Name);
        }

        [TestMethod]
        public void FromPassive_IconPathFromImage()
        {
            var vm = SpellViewModel.FromPassive(MakePassive());
            Assert.AreEqual("AhriPassive.png", vm.IconPath);
        }

        [TestMethod]
        public void FromPassive_ImageKind_IsPassive()
        {
            // Le passif est servi depuis /img/passive/, pas /img/spell/.
            var vm = SpellViewModel.FromPassive(MakePassive());
            Assert.AreEqual(LOLInfo.Utils.ImageConstant.PASSIVE, vm.ImageKind);
        }

        [TestMethod]
        public void FromSpell_ImageKind_IsSpell()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            Assert.AreEqual(LOLInfo.Utils.ImageConstant.SPELL, vm.ImageKind);
        }

        [TestMethod]
        public void FromPassive_NoStatRows()
        {
            var vm = SpellViewModel.FromPassive(MakePassive());
            Assert.AreEqual(0, vm.StatRows.Count);
        }

        // ── FromSpell ──────────────────────────────────────────────────────

        [TestMethod]
        public void FromSpell_KeyIsSet()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            Assert.AreEqual("Q", vm.Key);
        }

        [TestMethod]
        public void FromSpell_IsPassiveFalse()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            Assert.IsFalse(vm.IsPassive);
        }

        [TestMethod]
        public void FromSpell_StatRows_ContainsCooldown()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(cooldown: "9/8/7/6/5"));
            var row = vm.StatRows[0];
            StringAssert.Contains(row.Label, "Recharge");
            Assert.AreEqual(5, row.Values.Count);
        }

        [TestMethod]
        public void FromSpell_StatRows_FiltersCostZero()
        {
            // CostBurn "0" → pas de ligne coût
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(cost: "0"));
            Assert.IsFalse(vm.StatRows.Any(r => r.Label.Contains("Coût")));
        }

        [TestMethod]
        public void FromSpell_StatRows_FiltersRangeZero()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(range: "0"));
            Assert.IsFalse(vm.StatRows.Any(r => r.Label.Contains("Portée")));
        }

        [TestMethod]
        public void FromSpell_StatRows_HasThreeRows_WhenAllPresent()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(cooldown: "9", cost: "60", range: "550"));
            Assert.AreEqual(3, vm.StatRows.Count);
        }

        [TestMethod]
        public void FromSpell_HasStats_True_WhenCooldownPresent()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(cooldown: "9"));
            Assert.IsTrue(vm.HasStats);
        }

        // ── LevelHeaders ───────────────────────────────────────────────────

        [TestMethod]
        public void LevelHeaders_FiveRanks_ReturnsFiveHeaders()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(cooldown: "9/8/7/6/5"));
            CollectionAssert.AreEqual(
                new[] { "Niv 1", "Niv 2", "Niv 3", "Niv 4", "Niv 5" },
                vm.LevelHeaders.ToArray());
        }

        [TestMethod]
        public void LevelHeaders_Empty_WhenNoStatRows()
        {
            var vm = SpellViewModel.Empty("Q");
            Assert.AreEqual(0, vm.LevelHeaders.Count);
        }

        [TestMethod]
        public void LevelHeaders_ThreeRanks_ReturnsThreeHeaders()
        {
            var vm = SpellViewModel.FromSpell("R", MakeSpell(cooldown: "100/80/60"));
            Assert.AreEqual(3, vm.LevelHeaders.Count);
        }

        // ── LeveltipRows ───────────────────────────────────────────────────

        [TestMethod]
        public void LeveltipRows_WithValues_ReturnsRows()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(
                leveltipLabels: new List<string> { "Dégâts", "Recharge" },
                leveltipEffects: new List<string> { "60/95/130/165/200", "9/8/7/6/5" }));
            Assert.AreEqual(2, vm.LeveltipRows.Count);
        }

        [TestMethod]
        public void LeveltipRows_FiltersTemplateVariables()
        {
            // Les effets "{{ basedamage }}" doivent être filtrés
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(
                leveltipLabels: new List<string> { "Dégâts", "Recharge" },
                leveltipEffects: new List<string> { "{{ basedamage }}", "9/8/7/6/5" }));
            Assert.AreEqual(1, vm.LeveltipRows.Count);
            Assert.AreEqual("9/8/7/6/5", vm.LeveltipRows[0].Effect);
        }

        [TestMethod]
        public void LeveltipRows_NullLeveltip_ReturnsEmpty()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            Assert.AreEqual(0, vm.LeveltipRows.Count);
        }

        [TestMethod]
        public void HasLeveltip_True_WhenRowsExist()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell(
                leveltipLabels: new List<string> { "Dégâts" },
                leveltipEffects: new List<string> { "60/95/130" }));
            Assert.IsTrue(vm.HasLeveltip);
        }

        // ── WithFormulas ───────────────────────────────────────────────────

        [TestMethod]
        public void WithFormulas_AddsFormulaRows()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            var calcs = new Dictionary<string, SpellCalculation>
            {
                ["BaseDamage"] = new SpellCalculation("BaseDamage", new List<IFormulaPart>
                {
                    new NumberPart(100)
                })
            };

            var enriched = vm.WithFormulas(calcs);

            Assert.AreEqual(1, enriched.FormulaRows.Count);
            Assert.AreEqual("Base Damage", enriched.FormulaRows[0].Label); // nom de calcul humanisé
            Assert.AreEqual("100", enriched.FormulaRows[0].Formula);
        }

        [TestMethod]
        public void WithFormulas_NullCalcs_ReturnsSelf()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            var result = vm.WithFormulas(null);
            Assert.AreSame(vm, result);
        }

        [TestMethod]
        public void WithFormulas_EmptyCalcs_ReturnsSelf()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            var result = vm.WithFormulas(new Dictionary<string, SpellCalculation>());
            Assert.AreSame(vm, result);
        }

        [TestMethod]
        public void WithFormulas_PreservesOriginalFields()
        {
            var original = SpellViewModel.FromSpell("Q", MakeSpell(name: "Orbe"));
            var calcs = new Dictionary<string, SpellCalculation>
            {
                ["X"] = new SpellCalculation("X", new List<IFormulaPart> { new NumberPart(1) })
            };

            var enriched = original.WithFormulas(calcs);

            Assert.AreEqual("Orbe", enriched.Name);
            Assert.AreEqual("Q", enriched.Key);
            CollectionAssert.AreEqual(original.StatRows.ToList(), enriched.StatRows.ToList());
        }

        [TestMethod]
        public void WithFormulas_FilterEmptyFormulas()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            // Une formule avec une partie vide ne doit pas créer de ligne
            var calcs = new Dictionary<string, SpellCalculation>
            {
                ["Empty"] = new SpellCalculation("Empty", new List<IFormulaPart>())
            };

            var enriched = vm.WithFormulas(calcs);
            Assert.AreEqual(0, enriched.FormulaRows.Count);
        }

        [TestMethod]
        public void HasFormulas_True_AfterWithFormulas()
        {
            var vm = SpellViewModel.FromSpell("Q", MakeSpell());
            var calcs = new Dictionary<string, SpellCalculation>
            {
                ["X"] = new SpellCalculation("X", new List<IFormulaPart> { new NumberPart(50) })
            };
            var enriched = vm.WithFormulas(calcs);
            Assert.IsTrue(enriched.HasFormulas);
        }
    }
}
