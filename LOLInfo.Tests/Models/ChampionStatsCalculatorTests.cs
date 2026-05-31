namespace LOLInfo.Tests.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using LOLInfo.Models;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChampionStatsCalculatorTests
    {
        [TestMethod]
        public void GrowthMultiplier_Level1_IsZero()
            => Assert.AreEqual(0.0, ChampionStatsCalculator.GrowthMultiplier(1), 1e-9);

        [TestMethod]
        public void GrowthMultiplier_Level18_Is17()
            => Assert.AreEqual(17.0, ChampionStatsCalculator.GrowthMultiplier(18), 1e-9);

        [TestMethod]
        public void GrowthMultiplier_Level6_Is3point95()
            => Assert.AreEqual(3.95, ChampionStatsCalculator.GrowthMultiplier(6), 1e-9);

        [TestMethod]
        public void PerLevel_MatchesInGameValues()
        {
            // Garen : HP 690 (+98/niv) → 690 au niv 1, 1077.1 au niv 6, 2356 au niv 18.
            Assert.AreEqual(690.0,  ChampionStatsCalculator.PerLevel(690, 98, 1),  1e-6);
            Assert.AreEqual(1077.1, ChampionStatsCalculator.PerLevel(690, 98, 6),  1e-6);
            Assert.AreEqual(2356.0, ChampionStatsCalculator.PerLevel(690, 98, 18), 1e-6);
        }

        [TestMethod]
        public void AttackSpeed_GrowsByPercent()
        {
            // base 0.625, +3.65%/niv : niv 1 = base, niv 18 = 0.625*(1+0.0365*17).
            Assert.AreEqual(0.625, ChampionStatsCalculator.AttackSpeed(0.625, 3.65, 1), 1e-9);
            Assert.AreEqual(0.625 * (1 + 0.0365 * 17),
                            ChampionStatsCalculator.AttackSpeed(0.625, 3.65, 18), 1e-9);
        }

        [TestMethod]
        public void Compute_NullStats_ReturnsEmpty()
            => Assert.AreEqual(0, ChampionStatsCalculator.Compute(null, 5).Count);

        [TestMethod]
        public void Compute_ReturnsAllStatKinds()
        {
            var stats = new Dictionary<string, double> { ["hp"] = 600 };
            var result = ChampionStatsCalculator.Compute(stats, 1);
            Assert.AreEqual(13, result.Count);
            Assert.IsTrue(result.Any(r => r.Kind == ChampionStatKind.Health));
            Assert.IsTrue(result.Any(r => r.Kind == ChampionStatKind.AttackSpeed));
        }

        [TestMethod]
        public void Compute_ClampsLevelTo1To18()
        {
            var stats = new Dictionary<string, double> { ["hp"] = 600, ["hpperlevel"] = 100 };
            var low  = ChampionStatsCalculator.Compute(stats, 0)[0].Value;   // borné à 1 → 600
            var high = ChampionStatsCalculator.Compute(stats, 99)[0].Value;  // borné à 18 → 2300
            Assert.AreEqual(600.0,  low,  1e-6);
            Assert.AreEqual(2300.0, high, 1e-6);
        }

        [TestMethod]
        public void Compute_WithItemBonuses_AddsFlatAndPercent()
        {
            var stats = new Dictionary<string, double>
            {
                ["attackdamage"] = 60, ["attackspeed"] = 0.625, ["movespeed"] = 340, ["hp"] = 600,
            };
            var items = new Dictionary<string, double>
            {
                ["FlatPhysicalDamageMod"] = 70,
                ["FlatMagicDamageMod"]    = 80,
                ["FlatHPPoolMod"]         = 200,
                ["PercentAttackSpeedMod"] = 0.40,
                ["PercentLifeStealMod"]   = 0.12,
                ["FlatMovementSpeedMod"]  = 45,
            };

            var r = ChampionStatsCalculator.Compute(stats, 1, items);
            double Val(ChampionStatKind k) => r.First(x => x.Kind == k).Value;

            Assert.AreEqual(130.0, Val(ChampionStatKind.AttackDamage), 1e-6); // 60 + 70
            Assert.AreEqual(80.0,  Val(ChampionStatKind.AbilityPower), 1e-6); // 0 + 80
            Assert.AreEqual(800.0, Val(ChampionStatKind.Health),       1e-6); // 600 + 200
            Assert.AreEqual(0.625 * 1.40, Val(ChampionStatKind.AttackSpeed), 1e-9); // niv1 + 40%
            Assert.AreEqual(0.12,  Val(ChampionStatKind.LifeSteal),    1e-9);
            Assert.AreEqual(385.0, Val(ChampionStatKind.MoveSpeed),    1e-6); // 340 + 45
        }

        [TestMethod]
        public void Compute_MoveSpeedAndRange_DoNotScale()
        {
            var stats = new Dictionary<string, double> { ["movespeed"] = 340, ["attackrange"] = 175 };
            var l18 = ChampionStatsCalculator.Compute(stats, 18);
            Assert.AreEqual(340.0, l18.First(r => r.Kind == ChampionStatKind.MoveSpeed).Value, 1e-9);
            Assert.AreEqual(175.0, l18.First(r => r.Kind == ChampionStatKind.AttackRange).Value, 1e-9);
        }
    }
}
