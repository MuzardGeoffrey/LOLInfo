namespace LOLInfo.Tests.CdragonModel.Parts
{
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AbilityResourceCoefficientPartTests
    {
        [TestMethod]
        public void Format_IntegerPercent_NoDecimal()
        {
            var part = new AbilityResourceCoefficientPart(0.02);
            Assert.AreEqual("+2% ressource", part.Format());
        }

        [TestMethod]
        public void Format_FractionalPercent_ShowsDecimals()
        {
            var part = new AbilityResourceCoefficientPart(0.025);
            Assert.AreEqual("+2.5% ressource", part.Format());
        }

        [TestMethod]
        public void Format_ZeroCoeff_ShowsZero()
        {
            var part = new AbilityResourceCoefficientPart(0.0);
            Assert.AreEqual("+0% ressource", part.Format());
        }

        [TestMethod]
        public void Coefficient_IsStoredCorrectly()
        {
            var part = new AbilityResourceCoefficientPart(0.10);
            Assert.AreEqual(0.10, part.Coefficient);
        }
    }
}
