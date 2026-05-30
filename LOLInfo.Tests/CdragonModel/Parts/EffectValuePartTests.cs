namespace LOLInfo.Tests.CdragonModel.Parts
{
    using System.Collections.Generic;
    using System.Linq;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EffectValuePartTests
    {
        private static readonly IReadOnlyList<IReadOnlyList<double>> SampleEffect =
            new List<IReadOnlyList<double>>
            {
                new List<double> { 0 },
                new List<double> { 60, 95, 130, 165, 200 },
                new List<double> { 0.3, 0.4, 0.5 },
            };

        [TestMethod]
        public void Format_IntegerValues_NoDecimal()
        {
            var part = new EffectValuePart(1, SampleEffect);
            Assert.AreEqual("60/95/130/165/200", part.Format());
        }

        [TestMethod]
        public void Format_DecimalValues_TwoSignificantFigures()
        {
            var part = new EffectValuePart(2, SampleEffect);
            Assert.AreEqual("0.3/0.4/0.5", part.Format());
        }

        [TestMethod]
        public void Format_OutOfBoundsIndex_ReturnsQuestionMark()
        {
            var part = new EffectValuePart(99, SampleEffect);
            Assert.AreEqual("?", part.Format());
        }

        [TestMethod]
        public void Format_NullEffectAmount_ReturnsQuestionMark()
        {
            var part = new EffectValuePart(1, null);
            Assert.AreEqual("?", part.Format());
        }

        [TestMethod]
        public void EffectIndex_IsStoredCorrectly()
        {
            var part = new EffectValuePart(1, SampleEffect);
            Assert.AreEqual(1, part.EffectIndex);
        }

        [TestMethod]
        public void Values_AreResolvedCorrectly()
        {
            var part = new EffectValuePart(1, SampleEffect);
            CollectionAssert.AreEqual(
                new[] { 60.0, 95.0, 130.0, 165.0, 200.0 },
                part.Values.ToArray());
        }
    }
}
