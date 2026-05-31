namespace LOLInfo.Models.CdragonModel;

using LOLInfo.Properties;

public enum ChampionStat
{
    Unknown       = 0,
    AttackDamage  = 1,
    AttackSpeed   = 2,
    AbilityPower  = 3,
    Armor         = 4,
    MagicResist   = 5,
    Health        = 6,
    MovementSpeed = 7,
    CritChance    = 8,
    HealthRegen   = 9,
    Mana          = 10,
    ManaRegen     = 11,
    AbilityHaste  = 12,
}

public enum StatFormula
{
    Base  = 0,
    Bonus = 1,
    Total = 2,
}

public static class ChampionStatExtensions
{
    public static string ToLabel(this ChampionStat stat, StatFormula formula = StatFormula.Total)
    {
        string statName = stat switch
        {
            ChampionStat.AttackDamage  => Resources.Stat_AttackDamage,
            ChampionStat.AttackSpeed   => Resources.Stat_AttackSpeed,
            ChampionStat.AbilityPower  => Resources.Stat_AbilityPower,
            ChampionStat.Armor         => Resources.Stat_Armor,
            ChampionStat.MagicResist   => Resources.Stat_MagicResist,
            ChampionStat.Health        => Resources.Stat_Health,
            ChampionStat.MovementSpeed => Resources.Stat_MovementSpeed,
            ChampionStat.CritChance    => Resources.Stat_CritChance,
            ChampionStat.HealthRegen   => Resources.Stat_HealthRegen,
            ChampionStat.Mana          => Resources.Stat_Mana,
            ChampionStat.ManaRegen     => Resources.Stat_ManaRegen,
            ChampionStat.AbilityHaste  => Resources.Stat_AbilityHaste,
            _                          => Resources.Stat_Unknown,
        };

        string formulaSuffix = formula switch
        {
            StatFormula.Base  => Resources.StatSuffix_Base,
            StatFormula.Bonus => Resources.StatSuffix_Bonus,
            _                 => string.Empty,
        };

        return $"{statName}{formulaSuffix}";
    }
}
