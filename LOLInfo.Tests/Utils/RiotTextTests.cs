namespace LOLInfo.Tests.Utils
{
    using LOLInfo.Utils;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RiotTextTests
    {
        [DataTestMethod]
        [DataRow("Inflige <br>des dégâts", "Inflige \ndes dégâts")]
        [DataRow("a<br/>b", "a\nb")]
        [DataRow("a<br />b", "a\nb")]
        [DataRow("a<BR>b", "a\nb")]
        [DataRow("a< br >b", "a\nb")]
        public void CleanDescription_ReplacesBrVariantsWithNewline(string input, string expected)
            => Assert.AreEqual(expected, RiotText.CleanDescription(input));

        [TestMethod]
        public void CleanDescription_NullOrEmpty_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, RiotText.CleanDescription(null));
            Assert.AreEqual(string.Empty, RiotText.CleanDescription(""));
        }

        [TestMethod]
        public void CleanDescription_NoTag_TrimmedButUnchanged()
            => Assert.AreEqual("Orbe", RiotText.CleanDescription("  Orbe  "));

        [TestMethod]
        public void StripHtml_RemovesTags_KeepsText()
        {
            var r = RiotText.StripHtml("<mainText><stats>AD</stats><br>Inflige des <b>dégâts</b>.</mainText>");
            Assert.IsFalse(r.Contains("<"));
            StringAssert.Contains(r, "AD");
            StringAssert.Contains(r, "Inflige des dégâts.");
        }

        [TestMethod]
        public void StripHtml_DecodesEntities()
            => Assert.AreEqual("a & b", RiotText.StripHtml("a &amp; b"));

        [TestMethod]
        public void StripHtml_NullOrEmpty_ReturnsEmpty()
            => Assert.AreEqual(string.Empty, RiotText.StripHtml(null));

        [DataTestMethod]
        [DataRow("TotalDamage", "Total Damage")]
        [DataRow("totalDamage", "Total Damage")]
        [DataRow("RegenCalc", "Regen Calc")]
        [DataRow("NumberOfStrikes", "Number Of Strikes")]
        [DataRow("Damage", "Damage")]
        public void Humanize_SplitsCamelCase(string input, string expected)
            => Assert.AreEqual(expected, RiotText.Humanize(input));
    }
}
