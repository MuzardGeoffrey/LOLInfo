namespace LOLInfo.Models;

using System;
using System.Collections.Generic;

/// <summary>Catégories de statistiques d'un champion, dans l'ordre d'affichage.</summary>
public enum ChampionStatKind
{
    Health,
    HealthRegen,
    Mana,
    ManaRegen,
    Armor,
    MagicResist,
    AttackDamage,
    AbilityPower,
    AttackSpeed,
    CritChance,
    LifeSteal,
    MoveSpeed,
    AttackRange,
}

/// <summary>Une statistique calculée pour un niveau donné.</summary>
public readonly record struct ChampionStatValue(ChampionStatKind Kind, double Value);

/// <summary>
/// Calcule les statistiques d'un champion à un niveau donné depuis les stats de base
/// DataDragon (<c>Champion.Stats</c>).
///
/// Formule de croissance officielle de League of Legends :
///   valeur = base + parNiveau × (n−1) × (0,7025 + 0,0175 × (n−1))
/// (sous-linéaire entre 1 et 18, exactement linéaire au niveau 18).
/// La vitesse d'attaque suit la même croissance mais en pourcentage de la base.
/// </summary>
public static class ChampionStatsCalculator
{
    public const int MinLevel = 1;
    public const int MaxLevel = 18;

    /// <summary>Multiplicateur de croissance : (n−1)·(0,7025 + 0,0175·(n−1)). 0 au niv. 1.</summary>
    public static double GrowthMultiplier(int level) =>
        (level - 1) * (0.7025 + 0.0175 * (level - 1));

    /// <summary>base + parNiveau × croissance.</summary>
    public static double PerLevel(double baseValue, double perLevel, int level) =>
        baseValue + perLevel * GrowthMultiplier(level);

    /// <summary>Vitesse d'attaque : baseAS × (1 + parNiveau%/100 × croissance).</summary>
    public static double AttackSpeed(double baseAttackSpeed, double perLevelPercent, int level) =>
        baseAttackSpeed * (1 + perLevelPercent / 100.0 * GrowthMultiplier(level));

    private static double Get(IReadOnlyDictionary<string, double> stats, string key) =>
        stats.TryGetValue(key, out var v) ? v : 0;

    /// <summary>
    /// Calcule toutes les stats du champion au niveau demandé (borné à [1, 18]),
    /// en ajoutant éventuellement les bonus des objets équipés.
    /// <paramref name="itemBonuses"/> agrège les stats brutes des objets
    /// (clés DataDragon : "FlatPhysicalDamageMod", "PercentAttackSpeedMod"…).
    /// Retourne une liste vide si <paramref name="stats"/> est null.
    /// </summary>
    public static IReadOnlyList<ChampionStatValue> Compute(
        IReadOnlyDictionary<string, double>? stats,
        int level,
        IReadOnlyDictionary<string, double>? itemBonuses = null)
    {
        if (stats is null) return [];

        level = Math.Clamp(level, MinLevel, MaxLevel);

        double Item(string key) =>
            itemBonuses is not null && itemBonuses.TryGetValue(key, out var v) ? v : 0;

        // Vitesse d'attaque : base*(1+croissance) + base*Σ(%AS objets).
        var baseAttackSpeed = Get(stats, "attackspeed");
        var attackSpeed = AttackSpeed(baseAttackSpeed, Get(stats, "attackspeedperlevel"), level)
                          + baseAttackSpeed * Item("PercentAttackSpeedMod");

        // Vitesse de déplacement : (base + Σ plat) * (1 + Σ %).
        var moveSpeed = (Get(stats, "movespeed") + Item("FlatMovementSpeedMod"))
                        * (1 + Item("PercentMovementSpeedMod"));

        return
        [
            new(ChampionStatKind.Health,       PerLevel(Get(stats, "hp"),           Get(stats, "hpperlevel"),           level) + Item("FlatHPPoolMod")),
            new(ChampionStatKind.HealthRegen,  PerLevel(Get(stats, "hpregen"),      Get(stats, "hpregenperlevel"),      level) + Item("FlatHPRegenMod")),
            new(ChampionStatKind.Mana,         PerLevel(Get(stats, "mp"),           Get(stats, "mpperlevel"),           level) + Item("FlatMPPoolMod")),
            new(ChampionStatKind.ManaRegen,    PerLevel(Get(stats, "mpregen"),      Get(stats, "mpregenperlevel"),      level) + Item("FlatMPRegenMod")),
            new(ChampionStatKind.Armor,        PerLevel(Get(stats, "armor"),        Get(stats, "armorperlevel"),        level) + Item("FlatArmorMod")),
            new(ChampionStatKind.MagicResist,  PerLevel(Get(stats, "spellblock"),   Get(stats, "spellblockperlevel"),   level) + Item("FlatSpellBlockMod")),
            new(ChampionStatKind.AttackDamage, PerLevel(Get(stats, "attackdamage"), Get(stats, "attackdamageperlevel"), level) + Item("FlatPhysicalDamageMod")),
            new(ChampionStatKind.AbilityPower, Item("FlatMagicDamageMod")),
            new(ChampionStatKind.AttackSpeed,  attackSpeed),
            new(ChampionStatKind.CritChance,   PerLevel(Get(stats, "crit"), Get(stats, "critperlevel"), level) + Item("FlatCritChanceMod")),
            new(ChampionStatKind.LifeSteal,    Item("PercentLifeStealMod")),
            new(ChampionStatKind.MoveSpeed,    moveSpeed),
            new(ChampionStatKind.AttackRange,  Get(stats, "attackrange")),
        ];
    }
}
