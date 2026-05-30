namespace LOLInfo.Tests.CdragonModel.Parts
{
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StatByCoefficientPartTests
    {
        [TestMethod]
        public void Format_AP_ShowsPercent()
        {
            // coeff=0.30, stat=3 (AP), formula=2 (Total) → "+30% PA"
            var part = new StatByCoefficientPart(0.30, 3, 2);
            Assert.AreEqual("+30% PA", part.Format());
        }

        [TestMethod]
        public void Format_AD_Bonus_ShowsBonusSuffix()
        {
            var part = new StatByCoefficientPart(0.60, 1, 1);
            Assert.AreEqual("+60% AD bonus", part.Format());
        }

        [TestMethod]
        public void Format_UnknownStat_ShowsPercentOnly()
        {
            // statId hors enum (ex: 99) → "stat inconnue"
            var part = new StatByCoefficientPart(0.25, 99, 2);
            StringAssert.StartsWith(part.Format(), "+25%");
        }

        [TestMethod]
        public void Format_IntegerPercent_NoDecimal()
        {
            var part = new StatByCoefficientPart(0.50, 3, 2);
            Assert.AreEqual("+50% PA", part.Format());
        }

        [TestMethod]
        public void Format_FractionalPercent_ShowsDecimals()
        {
            // 0.025 → "+2.5% PA"
            var part = new StatByCoefficientPart(0.025, 3, 2);
            Assert.AreEqual("+2.5% PA", part.Format());
        }
    }
}
