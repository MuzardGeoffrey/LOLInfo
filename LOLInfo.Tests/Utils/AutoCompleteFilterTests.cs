namespace LOLInfo.Tests.Utils
{
    using System.Linq;

    using LOLInfo.Utils;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AutoCompleteFilterTests
    {
        private static readonly string[] Names =
        [
            "Lame d'infini", "Lame du roi déchu", "Coiffe de Rabadon",
            "Larme de la déesse", "Bottes de vitesse", "Lame d'infini", // doublon volontaire
        ];

        private static string Self(string s) => s;

        [TestMethod]
        public void Match_EmptyQuery_ReturnsNothing()
        {
            var r = AutoCompleteFilter.Match(Names, "", Self, 8);
            Assert.AreEqual(0, r.Count);
        }

        [TestMethod]
        public void Match_FiltersBySubstring_CaseInsensitive()
        {
            var r = AutoCompleteFilter.Match(Names, "lame", Self, 8);
            Assert.IsTrue(r.All(n => n.Contains("Lame", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(r.Contains("Lame d'infini"));
            Assert.IsTrue(r.Contains("Lame du roi déchu"));
        }

        [TestMethod]
        public void Match_DeduplicatesByLabel()
        {
            var r = AutoCompleteFilter.Match(Names, "Lame d'infini", Self, 8);
            Assert.AreEqual(1, r.Count); // le doublon est fusionné
        }

        [TestMethod]
        public void Match_PrefixMatchesRankFirst()
        {
            // "de" est en préfixe de "déesse"? non. Utilisons "la" : préfixe de "Larme"/"Lame".
            var r = AutoCompleteFilter.Match(Names, "la", Self, 8);
            // Les préfixes (Lame*, Larme*) doivent précéder un éventuel substring interne.
            Assert.IsTrue(r.Count > 0);
            Assert.IsTrue(r[0].StartsWith("La", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void Match_RespectsMaxCount()
        {
            var r = AutoCompleteFilter.Match(Names, "e", Self, 2);
            Assert.AreEqual(2, r.Count);
        }

        [TestMethod]
        public void Match_NullSource_ReturnsEmpty()
        {
            var r = AutoCompleteFilter.Match<string>(null, "x", Self, 8);
            Assert.AreEqual(0, r.Count);
        }
    }
}
