namespace LOLInfo.Tests.CdragonModel.Parts
{
    using System.Collections.Generic;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ByLevelInterpolationPartTests
    {
        [TestMethod]
        public void Format_IntegerBounds_ShowsDash()
        {
            var part = new ByLevelInterpolationPart(75, 165);
            Assert.AreEqual("75–165 (niv. 1→18)", part.Format());
        }

        [TestMethod]
        public void Format_SameStartEnd_ShowsBothValues()
        {
            var part = new ByLevelInterpolationPart(100, 100);
            Assert.AreEqual("100–100 (niv. 1→18)", part.Format());
        }

        [TestMethod]
        public void Format_DecimalValues_TwoSignificantFigures()
        {
            var part = new ByLevelInterpolationPart(1.5, 3.25);
            Assert.AreEqual("1.5–3.25 (niv. 1→18)", part.Format());
        }
    }

    [TestClass]
    public class ByLevelFormulaPartTests
    {
        [TestMethod]
        public void Format_31Values_ShowsNiv1_9_18()
        {
            var values = new List<double>(31);
            for (int i = 0; i < 31; i++) values.Add((i + 1) * 10.0); // 10, 20, ..., 310

            var part = new ByLevelFormulaPart(values);
            // niv.1 = values[0]=10, niv.9 = values[8]=90, niv.18 = values[17]=180
            Assert.AreEqual("10/90/180 (niv.1/9/18)", part.Format());
        }

        [TestMethod]
        public void Format_EmptyList_ReturnsQuestionMark()
        {
            var part = new ByLevelFormulaPart(new List<double>());
            Assert.AreEqual("?", part.Format());
        }

        [TestMethod]
        public void Format_SingleValue_ReturnsValue()
        {
            var part = new ByLevelFormulaPart(new List<double> { 42 });
            Assert.AreEqual("42", part.Format());
        }
    }

    [TestClass]
    public class ByLevelBreakpointsPartTests
    {
        [TestMethod]
        public void Format_WithBreakpoints_ShowsCumulativeValues()
        {
            // Level1=55, breakpoints: niv7 → -5, niv13 → -5, niv19 → -5
            // Résultat cumulatif : 55 → 50 → 45 → 40
            var breakpoints = new List<(int, double)>
            {
                (7,  -5),
                (13, -5),
                (19, -5),
            };
            var part = new ByLevelBreakpointsPart(55, breakpoints);
            Assert.AreEqual("55→50→45→40 (par niv.)", part.Format());
        }

        [TestMethod]
        public void Format_NoBreakpoints_ReturnsLevel1Only()
        {
            var part = new ByLevelBreakpointsPart(100, new List<(int, double)>());
            Assert.AreEqual("100", part.Format());
        }

        [TestMethod]
        public void Format_DecimalValue_NoTrailingZeros()
        {
            var breakpoints = new List<(int, double)> { (6, -2.5) };
            var part = new ByLevelBreakpointsPart(10.5, breakpoints);
            Assert.AreEqual("10.5→8 (par niv.)", part.Format());
        }
    }
}
