namespace LOLInfo.Tests.Models
{
    using LOLInfo.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChampionResourcesTests
    {
        // ── GetCategory ────────────────────────────────────────────────────

        [DataTestMethod]
        [DataRow("Mana",    ChampionResources.Mana)]
        [DataRow("Aucune",  ChampionResources.Aucun)]
        [DataRow("Énergie", ChampionResources.Energie)]
        public void GetCategory_KnownPartypes_ReturnsCorrectCategory(string partype, string expected)
        {
            Assert.AreEqual(expected, ChampionResources.GetCategory(partype));
        }

        [DataTestMethod]
        [DataRow("Rage")]
        [DataRow("Courage")]
        [DataRow("Bouclier")]
        [DataRow("Afflux sanguin")]
        [DataRow("Agressivité")]
        public void GetCategory_UnknownPartypes_ReturnsAutre(string partype)
        {
            Assert.AreEqual(ChampionResources.Autre, ChampionResources.GetCategory(partype));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void GetCategory_NullOrEmpty_ReturnsAutre(string? partype)
        {
            Assert.AreEqual(ChampionResources.Autre, ChampionResources.GetCategory(partype));
        }

        // ── CanonicalOrder ─────────────────────────────────────────────────

        [TestMethod]
        public void CanonicalOrder_ContainsFourCategories()
        {
            Assert.AreEqual(4, ChampionResources.CanonicalOrder.Length);
        }

        [TestMethod]
        public void CanonicalOrder_ContainsAllExpectedCategories()
        {
            CollectionAssert.Contains(ChampionResources.CanonicalOrder, ChampionResources.Mana);
            CollectionAssert.Contains(ChampionResources.CanonicalOrder, ChampionResources.Aucun);
            CollectionAssert.Contains(ChampionResources.CanonicalOrder, ChampionResources.Energie);
            CollectionAssert.Contains(ChampionResources.CanonicalOrder, ChampionResources.Autre);
        }

        [TestMethod]
        public void CanonicalOrder_ManaIsFirst()
        {
            Assert.AreEqual(ChampionResources.Mana, ChampionResources.CanonicalOrder[0]);
        }
    }
}
