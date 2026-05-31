namespace LOLInfo.ViewModels;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using LOLInfo.Models;
using LOLInfo.Models.CdragonModel;
using LOLInfo.Models.RiotModel;
using LOLInfo.Properties;

/// <summary>
/// Wrapper bindable pour un sort (actif ou passif).
/// Unifie Passive et Spell dans un seul type pour le DataTemplate XAML.
///
/// Fabriques statiques :
///   SpellViewModel.Empty("Q")            ← placeholder avant chargement
///   SpellViewModel.FromPassive(passive)
///   SpellViewModel.FromSpell("Q", spell)
/// </summary>
public class SpellViewModel
{
    // ── Identité ──────────────────────────────────────────────────────────

    /// <summary>Touche d'activation : "Passif", "Q", "W", "E", "R".</summary>
    public string Key         { get; }
    public string Name        { get; }
    public string Description { get; }
    public string IconPath    { get; }

    /// <summary>True si ce sort est le passif du champion.</summary>
    public bool IsPassive { get; }

    // ── Tableau de progression par niveau ─────────────────────────────────

    public IReadOnlyList<string> LevelHeaders =>
        this.StatRows.Count > 0
            ? Enumerable.Range(1, this.StatRows[0].Values.Count)
                .Select(i => string.Format(CultureInfo.CurrentCulture, Resources.Spell_LevelHeader, i))
                .ToList<string>()
            : [];

    public IReadOnlyList<SpellStatRow>          StatRows     { get; }
    public IReadOnlyList<LeveltipRowViewModel>  LeveltipRows { get; }
    public IReadOnlyList<FormulaRowViewModel>   FormulaRows  { get; private set; }

    // ── Visibilité ────────────────────────────────────────────────────────

    public bool HasStats    => this.StatRows.Count     > 0;
    public bool HasLeveltip => this.LeveltipRows.Count > 0;
    public bool HasFormulas => this.FormulaRows.Count  > 0;

    // ── Constructeur privé ────────────────────────────────────────────────

    private SpellViewModel(
        string key,
        string name,
        string description,
        string iconPath,
        bool   isPassive,
        IReadOnlyList<SpellStatRow>         statRows,
        IReadOnlyList<LeveltipRowViewModel> leveltipRows)
    {
        this.Key          = key;
        this.Name         = name        ?? string.Empty;
        this.Description  = description ?? string.Empty;
        this.IconPath     = iconPath    ?? string.Empty;
        this.IsPassive    = isPassive;
        this.StatRows     = statRows;
        this.LeveltipRows = leveltipRows;
        this.FormulaRows  = [];
    }

    // ── Fabriques statiques ───────────────────────────────────────────────

    public static SpellViewModel Empty(string key) =>
        new(key, string.Empty, string.Empty, string.Empty, false, [], []);

    public static SpellViewModel FromPassive(Passive passive) =>
        new(SpellKeys.Passive,
            passive.Name        ?? string.Empty,
            passive.Description ?? string.Empty,
            passive.Image?.Full ?? string.Empty,
            true, [], []);

    public static SpellViewModel FromSpell(string key, Spell spell) =>
        new(key,
            spell.Name        ?? string.Empty,
            spell.Description ?? string.Empty,
            spell.Image?.Full  ?? string.Empty,
            false,
            BuildStatRows(spell),
            BuildLeveltipRows(spell));

    // ── Enrichissement CDragon ────────────────────────────────────────────

    public SpellViewModel WithFormulas(Dictionary<string, SpellCalculation>? calculations)
    {
        if (calculations is null || calculations.Count == 0) return this;

        var rows = calculations
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value.Format()))
            .Select(kv => new FormulaRowViewModel(kv.Key, kv.Value.Format()))
            .ToList<FormulaRowViewModel>();

        var clone = new SpellViewModel(this.Key, this.Name, this.Description, this.IconPath, this.IsPassive, this.StatRows, this.LeveltipRows);
        clone.FormulaRows = rows;
        return clone;
    }

    // ── Construction interne ──────────────────────────────────────────────

    private static IReadOnlyList<SpellStatRow> BuildStatRows(Spell spell)
    {
        List<SpellStatRow> rows = [];

        if (!string.IsNullOrWhiteSpace(spell.CooldownBurn))
            rows.Add(new SpellStatRow(Resources.Spell_Cooldown, spell.CooldownBurn));

        if (!string.IsNullOrWhiteSpace(spell.CostBurn) && spell.CostBurn != "0")
            rows.Add(new SpellStatRow(Resources.Spell_Cost, spell.CostBurn));

        if (!string.IsNullOrWhiteSpace(spell.RangeBurn) && spell.RangeBurn != "0")
            rows.Add(new SpellStatRow(Resources.Spell_Range, spell.RangeBurn));

        return rows;
    }

    private static IReadOnlyList<LeveltipRowViewModel> BuildLeveltipRows(Spell spell)
    {
        var labels  = spell.Leveltip?.Label;
        var effects = spell.Leveltip?.Effect;

        if (labels is null || effects is null || labels.Count == 0) return [];

        return labels
            .Zip(effects, (label, effect) => new LeveltipRowViewModel(label, effect))
            .Where(row => !row.Effect.Contains("{{"))
            .ToList();
    }
}
