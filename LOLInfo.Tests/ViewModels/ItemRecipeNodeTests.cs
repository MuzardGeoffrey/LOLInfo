namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using LOLInfo.Models.RiotModel;
    using LOLInfo.ViewModels;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ItemRecipeNodeTests
    {
        private static Item MakeItem(string id, string name, params string[] from) => new()
        {
            Id    = id,
            Name  = name,
            Image = new Image { Full = id + ".png" },
            Gold  = new ItemGold { Total = 1000 },
            Stats = new Dictionary<string, double> { ["FlatPhysicalDamageMod"] = 10 },
            From  = from.Length == 0 ? null : from.ToList(),
        };

        private static Dictionary<string, ItemViewModel> Index(params Item[] items)
            => items.Select(ItemViewModel.From)
                    .ToDictionary(vm => vm.Id, vm => vm);

        [TestMethod]
        public void Build_ResolvesNestedComponentsRecursively()
        {
            // Infinity Edge ← (B.F. Sword ← Long Sword + Long Sword) + Pickaxe
            var longSword = MakeItem("1036", "Long Sword");
            var bfSword   = MakeItem("1038", "B.F. Sword", "1036", "1036");
            var pickaxe   = MakeItem("3057", "Pickaxe");
            var ie        = MakeItem("3031", "Infinity Edge", "1038", "3057");

            var byId = Index(longSword, bfSword, pickaxe, ie);

            var root = ItemRecipeNode.Build(byId["3031"], byId);

            Assert.AreEqual("Infinity Edge", root.Name);
            Assert.AreEqual(2, root.Components.Count);

            var firstChild = root.Components[0];
            Assert.AreEqual("B.F. Sword", firstChild.Name);
            Assert.AreEqual(2, firstChild.Components.Count);
            Assert.IsTrue(firstChild.Components.All(c => c.Name == "Long Sword"));

            Assert.AreEqual("Pickaxe", root.Components[1].Name);
            Assert.IsFalse(root.Components[1].HasComponents);
        }

        [TestMethod]
        public void Build_SetsFirstAndLastFlagsOnSiblings()
        {
            var a = MakeItem("1", "A");
            var b = MakeItem("2", "B");
            var c = MakeItem("3", "C");
            var root = MakeItem("9", "Root", "1", "2", "3");

            var node = ItemRecipeNode.Build(Index(a, b, c, root)["9"], Index(a, b, c, root));

            Assert.IsTrue(node.Components[0].IsFirst);
            Assert.IsFalse(node.Components[0].IsLast);
            Assert.IsFalse(node.Components[1].IsFirst);
            Assert.IsFalse(node.Components[1].IsLast);
            Assert.IsFalse(node.Components[2].IsFirst);
            Assert.IsTrue(node.Components[2].IsLast);

            // ShowLeftBar / ShowRightBar pilotent les demi-barres du connecteur.
            Assert.IsFalse(node.Components[0].ShowLeftBar);
            Assert.IsTrue(node.Components[0].ShowRightBar);
            Assert.IsTrue(node.Components[2].ShowLeftBar);
            Assert.IsFalse(node.Components[2].ShowRightBar);
        }

        [TestMethod]
        public void Build_SingleComponent_HasNoBars()
        {
            var leaf = MakeItem("1", "Leaf");
            var root = MakeItem("9", "Root", "1");

            var node = ItemRecipeNode.Build(Index(leaf, root)["9"], Index(leaf, root));

            Assert.AreEqual(1, node.Components.Count);
            Assert.IsTrue(node.Components[0].IsFirst);
            Assert.IsTrue(node.Components[0].IsLast);
            Assert.IsFalse(node.Components[0].ShowLeftBar);
            Assert.IsFalse(node.Components[0].ShowRightBar);
        }

        [TestMethod]
        public void Build_UnknownComponentId_IsSkipped()
        {
            var known = MakeItem("1", "Known");
            var root  = MakeItem("9", "Root", "1", "9999"); // 9999 absent de l'index

            var node = ItemRecipeNode.Build(Index(known, root)["9"], Index(known, root));

            Assert.AreEqual(1, node.Components.Count);
            Assert.AreEqual("Known", node.Components[0].Name);
        }

        [TestMethod]
        public void Build_BasicItem_HasNoComponents()
        {
            var basic = MakeItem("1036", "Long Sword");

            var node = ItemRecipeNode.Build(Index(basic)["1036"], Index(basic));

            Assert.IsFalse(node.HasComponents);
        }
    }
}
