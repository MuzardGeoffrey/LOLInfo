namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ItemViewModelTests
    {
        private static Item MakeItem(Dictionary<string, double> stats, string description = "") => new()
        {
            Id = "3031",
            Name = "Infinity Edge",
            Image = new Image { Full = "3031.png" },
            Gold = new ItemGold { Total = 3300 },
            Stats = stats,
            Description = description,
        };

        [TestMethod]
        public void From_MapsBasicFields()
        {
            var vm = ItemViewModel.From(MakeItem(new() { ["FlatPhysicalDamageMod"] = 70 }));
            Assert.AreEqual("Infinity Edge", vm.Name);
            Assert.AreEqual("3031.png", vm.ImagePath);
            Assert.AreEqual(3300, vm.Gold);
        }

        [TestMethod]
        public void From_MapsStatsToLocalizedLabelsAndValues()
        {
            // Culture de test = fr.
            var vm = ItemViewModel.From(MakeItem(new()
            {
                ["FlatPhysicalDamageMod"] = 70,
                ["FlatCritChanceMod"]     = 0.20,
                ["FlatMagicDamageMod"]    = 80,
            }));

            Assert.AreEqual(3, vm.Stats.Count);
            Assert.AreEqual("+70",   vm.Stats.First(s => s.Label == "Dégâts d'attaque").Value);
            Assert.AreEqual("+20 %", vm.Stats.First(s => s.Label == "Chance de critique").Value);
            Assert.AreEqual("+80",   vm.Stats.First(s => s.Label == "Puissance").Value);
        }

        [TestMethod]
        public void From_IgnoresZeroValuedStats()
        {
            var vm = ItemViewModel.From(MakeItem(new()
            {
                ["FlatPhysicalDamageMod"] = 70,
                ["FlatMagicDamageMod"]    = 0,
            }));
            Assert.AreEqual(1, vm.Stats.Count);
        }

        [TestMethod]
        public void From_UnknownStatKey_IsHumanized()
        {
            var vm = ItemViewModel.From(MakeItem(new() { ["FlatWeirdCustomMod"] = 5 }));
            Assert.AreEqual("Flat Weird Custom Mod", vm.Stats[0].Label);
            Assert.AreEqual("+5", vm.Stats[0].Value);
        }

        [TestMethod]
        public void From_StripsHtmlFromEffects()
        {
            var vm = ItemViewModel.From(MakeItem(
                new() { ["FlatPhysicalDamageMod"] = 70 },
                "<mainText><stats>AD</stats><br>Inflige des <attention>dégâts</attention> bonus.</mainText>"));

            Assert.IsFalse(vm.Effects.Contains("<"), "Le HTML doit être retiré");
            StringAssert.Contains(vm.Effects, "dégâts bonus");
            Assert.IsTrue(vm.HasEffects);
            Assert.IsTrue(vm.HasStats);
        }
    }
}
