namespace LOLInfo.Tests.CdragonModel.Parts
{
    using System.Collections.Generic;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BuffCounterDataValuePartTests
    {
        [TestMethod]
        public void Format_MultipleValues_ShowsStacksTimesSlash()
        {
            var part = new BuffCounterDataValuePart(new List<double> { 1.5, 2.0, 2.5 });
            Assert.AreEqual("[stacks]×1.5/2/2.5", part.Format());
        }

        [TestMethod]
        public void Format_SingleValue_ShowsStacksTimesSingle()
        {
            var part = new BuffCounterDataValuePart(new List<double> { 3.0 });
            Assert.AreEqual("[stacks]×3", part.Format());
        }

        [TestMethod]
        public void Format_EmptyList_ReturnsUnknownPattern()
        {
            var part = new BuffCounterDataValuePart(new List<double>());
            Assert.AreEqual("[stacks]×?", part.Format());
        }
    }

    [TestClass]
    public class BuffCounterCoefficientPartTests
    {
        [TestMethod]
        public void Format_IntegerCoeff_NoDecimal()
        {
            var part = new BuffCounterCoefficientPart(2.0);
            Assert.AreEqual("[stacks]×2", part.Format());
        }

        [TestMethod]
        public void Format_DecimalCoeff_ShowsDecimal()
        {
            var part = new BuffCounterCoefficientPart(1.7);
            Assert.AreEqual("[stacks]×1.7", part.Format());
        }
    }
}
