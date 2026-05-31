namespace LOLInfo.ViewModels;

using System;
using System.Collections.Generic;
using System.Globalization;

using LOLInfo.Models.RiotModel;
using LOLInfo.Properties;
using LOLInfo.Utils;

/// <summary>
/// Wrapper d'affichage d'un objet : nom, icône, coût, stats (libellés localisés)
/// et effets (description nettoyée du HTML).
/// </summary>
public class ItemViewModel
{
    public string Id        { get; }
    public string Name      { get; }
    public string ImagePath { get; }
    public int    Gold      { get; }
    public string Effects   { get; }
    public IReadOnlyList<ChampionStatRow> Stats { get; }

    /// <summary>Stats brutes DataDragon (pour agréger les objets équipés sur un champion).</summary>
    public IReadOnlyDictionary<string, double> RawStats { get; }

    public bool HasStats   => this.Stats.Count > 0;
    public bool HasEffects => !string.IsNullOrWhiteSpace(this.Effects);

    private ItemViewModel(string id, string name, string imagePath, int gold, string effects,
                          IReadOnlyList<ChampionStatRow> stats, IReadOnlyDictionary<string, double> rawStats)
    {
        this.Id        = id;
        this.Name      = name;
        this.ImagePath = imagePath;
        this.Gold      = gold;
        this.Effects   = effects;
        this.Stats     = stats;
        this.RawStats  = rawStats;
    }

    public static ItemViewModel From(Item item)
    {
        var rows = new List<ChampionStatRow>();
        foreach (var (key, raw) in item.Stats ?? [])
        {
            if (raw == 0) continue;
            var (label, value) = FormatStat(key, raw);
            rows.Add(new ChampionStatRow(label, value));
        }

        return new ItemViewModel(
            item.Id   ?? string.Empty,
            item.Name ?? string.Empty,
            item.Image?.Full ?? string.Empty,
            item.Gold?.Total ?? 0,
            RiotText.StripHtml(item.Description),
            rows,
            item.Stats ?? new Dictionary<string, double>());
    }

    // ── Mapping des clés de stats DataDragon → libellé localisé ───────────────

    private static readonly Dictionary<string, (Func<string> Label, bool Percent)> StatMap = new()
    {
        ["FlatHPPoolMod"]           = (() => Resources.StatLabel_Health,       false),
        ["FlatMPPoolMod"]           = (() => Resources.StatLabel_Mana,         false),
        ["FlatPhysicalDamageMod"]   = (() => Resources.StatLabel_AttackDamage, false),
        ["FlatMagicDamageMod"]      = (() => Resources.StatLabel_AbilityPower, false),
        ["FlatArmorMod"]            = (() => Resources.StatLabel_Armor,        false),
        ["FlatSpellBlockMod"]       = (() => Resources.StatLabel_MagicResist,  false),
        ["PercentAttackSpeedMod"]   = (() => Resources.StatLabel_AttackSpeed,  true),
        ["FlatCritChanceMod"]       = (() => Resources.StatLabel_CritChance,   true),
        ["FlatMovementSpeedMod"]    = (() => Resources.StatLabel_MoveSpeed,    false),
        ["PercentMovementSpeedMod"] = (() => Resources.StatLabel_MoveSpeed,    true),
        ["PercentLifeStealMod"]     = (() => Resources.StatLabel_LifeSteal,    true),
        ["FlatHPRegenMod"]          = (() => Resources.StatLabel_HealthRegen,  true),
        ["FlatMPRegenMod"]          = (() => Resources.StatLabel_ManaRegen,    true),
    };

    private static (string Label, string Value) FormatStat(string key, double raw)
    {
        var c = CultureInfo.CurrentCulture;
        if (StatMap.TryGetValue(key, out var m))
        {
            var label = m.Label();
            var value = m.Percent
                ? "+" + (raw * 100).ToString("0.#", c) + " %"
                : "+" + raw.ToString("0.#", c);
            return (label, value);
        }

        // Clé inconnue : libellé humanisé + valeur brute (rien n'est perdu).
        return (RiotText.Humanize(key), "+" + raw.ToString("0.##", c));
    }
}
