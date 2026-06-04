namespace LOLInfo.ViewModels;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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

    /// <summary>Identifiants des composants directs (champ <c>from</c>) — base de l'arbre de fabrication.</summary>
    public IReadOnlyList<string> ComponentIds { get; }

    /// <summary>Disponibilité par carte (champ <c>maps</c> : id de carte → disponible).</summary>
    public IReadOnlyDictionary<string, bool> Maps { get; }

    /// <summary>Arbre de fabrication, renseigné après le chargement de tous les objets (cf. <see cref="SetRecipe"/>).</summary>
    public ItemRecipeNode? Recipe { get; private set; }

    public bool HasStats   => this.Stats.Count > 0;
    public bool HasEffects => !string.IsNullOrWhiteSpace(this.Effects);
    public bool HasRecipe  => this.Recipe is { HasComponents: true };

    private ItemViewModel(string id, string name, string imagePath, int gold, string effects,
                          IReadOnlyList<ChampionStatRow> stats, IReadOnlyDictionary<string, double> rawStats,
                          IReadOnlyList<string> componentIds, IReadOnlyDictionary<string, bool> maps)
    {
        this.Id           = id;
        this.Name         = name;
        this.ImagePath    = imagePath;
        this.Gold         = gold;
        this.Effects      = effects;
        this.Stats        = stats;
        this.RawStats     = rawStats;
        this.ComponentIds = componentIds;
        this.Maps         = maps;
    }

    /// <summary>
    /// True si l'objet est disponible sur la carte <paramref name="mapId"/>.
    /// Sans donnée de carte, l'objet est considéré disponible partout (on ne le cache pas).
    /// </summary>
    public bool IsAvailableOn(string mapId)
        => this.Maps.Count == 0 || (this.Maps.TryGetValue(mapId, out var ok) && ok);

    /// <summary>
    /// True si l'objet existe sur au moins une carte. Un objet présent sur aucune carte
    /// a été retiré du jeu (obsolète). Sans donnée de carte, on le considère présent.
    /// </summary>
    public bool IsOnAnyMap => this.Maps.Count == 0 || this.Maps.Values.Any(available => available);

    /// <summary>Associe l'arbre de fabrication construit une fois tous les objets connus.</summary>
    public void SetRecipe(ItemRecipeNode recipe) => this.Recipe = recipe;

    public static ItemViewModel From(Item item)
    {
        var rows = new List<ChampionStatRow>();
        foreach (var (key, raw) in item.Stats ?? [])
        {
            if (raw == 0) continue;
            var (label, value) = FormatStat(key, raw);
            rows.Add(new ChampionStatRow(label, value));
        }

        // Certaines stats (régén. de base du mana) n'existent que dans le texte du bloc
        // <stats>, pas dans le champ structuré. On les y récupère pour que le filtre et
        // le tri par statistique les prennent en compte (cf. ItemsViewModel).
        var rawStats = new Dictionary<string, double>(item.Stats ?? new Dictionary<string, double>());
        AddDescriptionStats(RiotText.ExtractStatsBlock(item.Description), rawStats);

        return new ItemViewModel(
            item.Id   ?? string.Empty,
            item.Name ?? string.Empty,
            item.Image?.Full ?? string.Empty,
            item.Gold?.Total ?? 0,
            RiotText.StripHtml(item.Description),
            rows,
            rawStats,
            item.From ?? [],
            item.Maps ?? new Dictionary<string, bool>());
    }

    // Mots-clés de « régénération du mana » par langue (le texte du bloc <stats> suit la
    // langue des données). "regen" couvre en/es/de/pt ; "régén" le fr ; "rigen" l'it.
    private static readonly string[] ManaRegenMarkers = ["regen", "régén", "rigen"];

    // Capture le pourcentage adjacent à un mot « régénération » (valeur pour le tri).
    private static readonly Regex RegenPercent =
        new(@"(\d+(?:[.,]\d+)?)\s*%?[^\d]{0,40}?(?:regen|régén|rigen)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Détecte la « régénération de base du mana » dans le résumé de stats (texte localisé)
    /// et l'ajoute comme <c>FlatMPRegenMod</c> — clé absente du champ structuré de DataDragon.
    /// </summary>
    private static void AddDescriptionStats(string statsBlock, Dictionary<string, double> rawStats)
    {
        if (statsBlock.Length == 0) return;
        if (rawStats.ContainsKey("FlatMPRegenMod")) return;

        var hasMana  = statsBlock.Contains("mana", StringComparison.OrdinalIgnoreCase);
        var hasRegen = ManaRegenMarkers.Any(m => statsBlock.Contains(m, StringComparison.OrdinalIgnoreCase));
        if (!hasMana || !hasRegen) return;

        var match = RegenPercent.Match(statsBlock);
        var value = match.Success &&
                    double.TryParse(match.Groups[1].Value.Replace(',', '.'),
                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var pct)
            ? pct
            : 1.0; // présence détectée mais pourcentage illisible : valeur sentinelle

        rawStats["FlatMPRegenMod"] = value;
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
