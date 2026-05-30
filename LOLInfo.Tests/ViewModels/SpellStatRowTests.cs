namespace LOLInfo.Tests.ViewModels
{
    using System.Linq;
    using LOLInfo.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SpellStatRowTests
    {
        [TestMethod]
        public void Constructor_SplitsBurnStringBySlash()
        {
            var row = new SpellStatRow("Recharge", "9/8/7/6/5");
            CollectionAssert.AreEqual(new[] { "9", "8", "7", "6", "5" }, row.Values.ToArray());
        }

        [TestMethod]
        public void Constructor_SingleValue_OnlyOneElement()
        {
            var row = new SpellStatRow("Recharge", "12");
            Assert.AreEqual(1, row.Values.Count);
            Assert.AreEqual("12", row.Values[0]);
        }

        [TestMethod]
        public void Constructor_NullBurnString_EmptyValues()
        {
            var row = new SpellStatRow("Recharge", null);
            Assert.AreEqual(0, row.Values.Count);
            Assert.IsFalse(row.HasValues);
        }

        [TestMethod]
        public void Constructor_EmptyBurnString_EmptyValues()
        {
            var row = new SpellStatRow("Recharge", string.Empty);
            Assert.AreEqual(0, row.Values.Count);
        }

        [TestMethod]
        public void HasValues_True_WhenBurnStringProvided()
        {
            var row = new SpellStatRow("Portée", "550");
            Assert.IsTrue(row.HasValues);
        }

        [TestMethod]
        public void Label_IsStoredCorrectly()
        {
            var row = new SpellStatRow("💧 Coût", "60/70/80");
            Assert.AreEqual("💧 Coût", row.Label);
        }
    }
}
