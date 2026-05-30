namespace LOLInfo.Tests.CdragonModel.Parts
{
    using System.Collections.Generic;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StatScalingPartTests
    {
        private static IReadOnlyDictionary<string, IReadOnlyList<double>> DataWith(string key, params double[] vals)
            => new Dictionary<string, IReadOnlyList<double>> { [key] = vals };

        [TestMethod]
        public void Format_AP_SingleRatio_ShowsPercent()
        {
            // mStat=3 (AP), mStatFormula=2 (Total), ratio=0.45 → "+45% PA"
            var part = new StatScalingPart(3, 2, "APRatio", DataWith("APRatio", 0.45));
            Assert.AreEqual("+45% PA", part.Format());
        }

        [TestMethod]
        public void Format_AD_BonusFormula_ShowsBonusSuffix()
        {
            // mStat=1 (AD), mStatFormula=1 (Bonus), ratio=0.60 → "+60% AD bonus"
            var part = new StatScalingPart(1, 1, "ADRatio", DataWith("ADRatio", 0.60));
            Assert.AreEqual("+60% AD bonus", part.Format());
        }

        [TestMethod]
        public void Format_MovementSpeed_NotPercent_ShowsRawValue()
        {
            // mStat=7 (vit. dépl.), mStatFormula=2, ratio=1.5 → "+1.5 vit. dépl."
            var part = new StatScalingPart(7, 2, "MSRatio", DataWith("MSRatio", 1.5));
            Assert.AreEqual("+1.5 vit. dépl.", part.Format());
        }

        [TestMethod]
        public void Format_MultipleRanks_ShowsSlashSeparated()
        {
            // ratios 0.3/0.4/0.5 → "+30/40/50% PA"
            var part = new StatScalingPart(3, 2, "APRatio", DataWith("APRatio", 0.3, 0.4, 0.5));
            Assert.AreEqual("+30/40/50% PA", part.Format());
        }

        [TestMethod]
        public void Format_MissingKey_ReturnsQuestionMark()
        {
            var part = new StatScalingPart(3, 2, "Missing", DataWith("Other", 0.5));
            StringAssert.StartsWith(part.Format(), "+?");
        }

        [TestMethod]
        public void Stat_IsResolvedFromMStatInt()
        {
            var part = new StatScalingPart(3, 2, "x", DataWith("x", 0.5));
            Assert.AreEqual(ChampionStat.AbilityPower, part.Stat);
        }

        [TestMethod]
        public void Formula_IsResolvedFromMStatFormulaInt()
        {
            var part = new StatScalingPart(3, 0, "x", DataWith("x", 0.5));
            Assert.AreEqual(StatFormula.Base, part.Formula);
        }
    }
}
