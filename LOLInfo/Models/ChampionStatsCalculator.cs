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
    AttackSpeed,
    MoveSpeed,
    AttackRange,
    CritChance,
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
    /// Calcule toutes les stats du champion au niveau demandé (borné à [1, 18]).
    /// Retourne une liste vide si <paramref name="stats"/> est null.
    /// </summary>
    public static IReadOnlyList<ChampionStatValue> Compute(
        IReadOnlyDictionary<string, double>? stats, int level)
    {
        if (stats is null) return [];

        level = Math.Clamp(level, MinLevel, MaxLevel);

        return
        [
            new(ChampionStatKind.Health,       PerLevel(Get(stats, "hp"),           Get(stats, "hpperlevel"),           level)),
            new(ChampionStatKind.HealthRegen,  PerLevel(Get(stats, "hpregen"),      Get(stats, "hpregenperlevel"),      level)),
            new(ChampionStatKind.Mana,         PerLevel(Get(stats, "mp"),           Get(stats, "mpperlevel"),           level)),
            new(ChampionStatKind.ManaRegen,    PerLevel(Get(stats, "mpregen"),      Get(stats, "mpregenperlevel"),      level)),
            new(ChampionStatKind.Armor,        PerLevel(Get(stats, "armor"),        Get(stats, "armorperlevel"),        level)),
            new(ChampionStatKind.MagicResist,  PerLevel(Get(stats, "spellblock"),   Get(stats, "spellblockperlevel"),   level)),
            new(ChampionStatKind.AttackDamage, PerLevel(Get(stats, "attackdamage"), Get(stats, "attackdamageperlevel"), level)),
            new(ChampionStatKind.AttackSpeed,  AttackSpeed(Get(stats, "attackspeed"), Get(stats, "attackspeedperlevel"), level)),
            new(ChampionStatKind.MoveSpeed,    Get(stats, "movespeed")),
            new(ChampionStatKind.AttackRange,  Get(stats, "attackrange")),
            new(ChampionStatKind.CritChance,   PerLevel(Get(stats, "crit"),         Get(stats, "critperlevel"),         level)),
        ];
    }
}
