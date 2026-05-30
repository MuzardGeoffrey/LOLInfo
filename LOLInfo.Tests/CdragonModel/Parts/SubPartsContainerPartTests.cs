namespace LOLInfo.Tests.CdragonModel.Parts
{
    using System.Collections.Generic;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProductOfSubPartsPartTests
    {
        [TestMethod]
        public void Format_TwoParts_ShowsMultiplication()
        {
            var parts = new List<IFormulaPart>
            {
                new NumberPart(2),
                new NumberPart(3),
            };
            var part = new ProductOfSubPartsPart(parts);
            Assert.AreEqual("(2)×(3)", part.Format());
        }

        [TestMethod]
        public void Format_EmptyList_ReturnsQuestionMark()
        {
            var part = new ProductOfSubPartsPart(new List<IFormulaPart>());
            Assert.AreEqual("?", part.Format());
        }
    }

    [TestClass]
    public class SumOfSubPartsPartTests
    {
        [TestMethod]
        public void Format_TwoParts_ShowsSum()
        {
            var parts = new List<IFormulaPart>
            {
                new NumberPart(10),
                new NumberPart(20),
            };
            var part = new SumOfSubPartsPart(parts);
            Assert.AreEqual("10 + 20", part.Format());
        }

        [TestMethod]
        public void Format_EmptyList_ReturnsQuestionMark()
        {
            var part = new SumOfSubPartsPart(new List<IFormulaPart>());
            Assert.AreEqual("?", part.Format());
        }
    }

    [TestClass]
    public class StatBySubPartPartTests
    {
        [TestMethod]
        public void Format_ShowsCoeffAndStat()
        {
            var subPart = new NumberPart(0.45);
            var part = new StatBySubPartPart(ChampionStat.AbilityPower, 2, subPart);
            // "(0.45) PA"
            StringAssert.Contains(part.Format(), "0.45");
            StringAssert.Contains(part.Format(), "PA");
        }

        [TestMethod]
        public void Format_NoStat_ShowsOnlyCoeff()
        {
            var subPart = new NumberPart(1.5);
            var part = new StatBySubPartPart((ChampionStat)(-1), 2, subPart);
            Assert.AreEqual("(1.5)", part.Format());
        }
    }

    [TestClass]
    public class ClampSubPartsPartTests
    {
        [TestMethod]
        public void Format_ThreeParts_ShowsClamp()
        {
            var parts = new List<IFormulaPart>
            {
                new NumberPart(50),
                new NumberPart(0),
                new NumberPart(100),
            };
            var part = new ClampSubPartsPart(parts);
            Assert.AreEqual("clamp(50, 0, 100)", part.Format());
        }

        [TestMethod]
        public void Format_TwoParts_ShowsTwoArgClamp()
        {
            var parts = new List<IFormulaPart> { new NumberPart(5), new NumberPart(10) };
            var part = new ClampSubPartsPart(parts);
            Assert.AreEqual("clamp(5, 10)", part.Format());
        }

        [TestMethod]
        public void Format_EmptyList_ReturnsQuestionMark()
        {
            var part = new ClampSubPartsPart(new List<IFormulaPart>());
            Assert.AreEqual("?", part.Format());
        }
    }
}
