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
