namespace LOLInfo.Models.CdragonModel;

using LOLInfo.Properties;

/// <summary>
/// Statistiques de champion. Les valeurs correspondent à l'enum <c>mStat</c> de
/// CDragon (utilisé dans les <c>StatBy…CalculationPart</c>), vérifié sur l'ensemble
/// des champions :
///   0=AP, 1=Armure, 2=AD, 4=Vit. attaque, 6=RM, 7=Vit. dépl., 8=Crit, 12=PV.
/// Quand <c>mStat</c> est ABSENT dans le JSON, la valeur par défaut est 0 (AP).
/// </summary>
public enum ChampionStat
{
    Unknown       = -1,
    AbilityPower  = 0,
    Armor         = 1,
    AttackDamage  = 2,
    AttackSpeed   = 4,
    MagicResist   = 6,
    MovementSpeed = 7,
    CritChance    = 8,
    Health        = 12,

    // Stats non rencontrées dans les ratios CDragon : valeurs synthétiques
    // (servent seulement à l'affichage/aux tests, ne correspondent à aucun mStat réel).
    Mana          = 100,
    ManaRegen     = 101,
    HealthRegen   = 102,
    AbilityHaste  = 103,
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
