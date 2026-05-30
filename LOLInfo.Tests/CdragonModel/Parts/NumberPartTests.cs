namespace LOLInfo.Tests.CdragonModel.Parts
{
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NumberPartTests
    {
        [DataTestMethod]
        [DataRow(5.0,   "5")]
        [DataRow(0.0,   "0")]
        [DataRow(-3.0,  "-3")]
        [DataRow(0.45,  "0.45")]
        [DataRow(1.5,   "1.5")]
        [DataRow(100.0, "100")]
        public void Format_ReturnsExpectedString(double value, string expected)
        {
            var part = new NumberPart(value);
            Assert.AreEqual(expected, part.Format());
        }

        [TestMethod]
        public void Evaluate_ReturnsValue()
        {
            var part = new NumberPart(3.14);
            Assert.AreEqual(3.14, part.Evaluate(new SpellContext()));
        }

        [TestMethod]
        public void Value_IsStoredCorrectly()
        {
            var part = new NumberPart(42.0);
            Assert.AreEqual(42.0, part.Value);
        }

        [TestMethod]
        public void Format_DecimalTruncatesTrailingZeros()
        {
            // 1.10 → "1.1" (pas "1.10")
            var part = new NumberPart(1.10);
            Assert.AreEqual("1.1", part.Format());
        }
    }
}
