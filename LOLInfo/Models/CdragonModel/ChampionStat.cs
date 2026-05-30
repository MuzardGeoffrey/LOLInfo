namespace LOLInfo.Models.CdragonModel;

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
            ChampionStat.AttackDamage  => "AD",
            ChampionStat.AttackSpeed   => "vit. attaque",
            ChampionStat.AbilityPower  => "PA",
            ChampionStat.Armor         => "armure",
            ChampionStat.MagicResist   => "RM",
            ChampionStat.Health        => "PV",
            ChampionStat.MovementSpeed => "vit. dépl.",
            ChampionStat.CritChance    => "chance crit.",
            ChampionStat.HealthRegen   => "régén. PV",
            ChampionStat.Mana          => "mana",
            ChampionStat.ManaRegen     => "régén. mana",
            ChampionStat.AbilityHaste  => "hâte",
            _                          => "stat inconnue",
        };

        string formulaSuffix = formula switch
        {
            StatFormula.Base  => " (base)",
            StatFormula.Bonus => " bonus",
            _                 => string.Empty,
        };

        return $"{statName}{formulaSuffix}";
    }
}
