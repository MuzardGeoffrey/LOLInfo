namespace LOLInfo.Tests.CdragonModel
{
    using LOLInfo.Models.CdragonModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChampionStatExtensionsTests
    {
        [DataTestMethod]
        [DataRow(ChampionStat.AttackDamage,  StatFormula.Total,  "AD")]
        [DataRow(ChampionStat.AttackDamage,  StatFormula.Bonus,  "AD bonus")]
        [DataRow(ChampionStat.AttackDamage,  StatFormula.Base,   "AD (base)")]
        [DataRow(ChampionStat.AbilityPower,  StatFormula.Total,  "PA")]
        [DataRow(ChampionStat.Armor,         StatFormula.Total,  "armure")]
        [DataRow(ChampionStat.MagicResist,   StatFormula.Total,  "RM")]
        [DataRow(ChampionStat.Health,        StatFormula.Total,  "PV")]
        [DataRow(ChampionStat.Mana,          StatFormula.Total,  "mana")]
        [DataRow(ChampionStat.MovementSpeed, StatFormula.Total,  "vit. dépl.")]
        [DataRow(ChampionStat.CritChance,    StatFormula.Total,  "chance crit.")]
        [DataRow(ChampionStat.HealthRegen,   StatFormula.Total,  "régén. PV")]
        [DataRow(ChampionStat.ManaRegen,     StatFormula.Total,  "régén. mana")]
        [DataRow(ChampionStat.AbilityHaste,  StatFormula.Total,  "hâte")]
        [DataRow(ChampionStat.Unknown,       StatFormula.Total,  "stat inconnue")]
        public void ToLabel_ReturnsExpectedLabel(ChampionStat stat, StatFormula formula, string expected)
        {
            Assert.AreEqual(expected, stat.ToLabel(formula));
        }

        [TestMethod]
        public void ToLabel_DefaultFormula_IsTotal()
        {
            // Sans argument, Total = pas de suffixe
            Assert.AreEqual("PA", ChampionStat.AbilityPower.ToLabel());
        }
    }
}
