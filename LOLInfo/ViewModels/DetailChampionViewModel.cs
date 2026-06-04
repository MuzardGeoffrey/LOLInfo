namespace LOLInfo.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using LOLInfo.IServices;
using LOLInfo.IViewModels;
using LOLInfo.Models;
using LOLInfo.Models.CdragonModel;
using LOLInfo.Models.RiotModel;
using LOLInfo.Properties;

using Microsoft.Extensions.Logging;

public class DetailChampionViewModel(
    IRiotClient httpRiot,
    ICdragonClient cdragon,
    IItemsViewModel items,
    string championName,
    ILogger<DetailChampionViewModel> logger) : BaseViewModel, IDetailChampionViewModel
{
    // ── Identité ──────────────────────────────────────────────────────────

    public string ChampionName { get; } = championName;

    // ── Champion chargé ───────────────────────────────────────────────────

    public Champion? Champion
    {
        get;
        private set
        {
            field = value;
            this.OnPropertyChanged(nameof(Champion));
        }
    }

    // ── Sorts ─────────────────────────────────────────────────────────────

    private List<SpellViewModel> _spells =
    [
        SpellViewModel.Empty(SpellKeys.Passive),
        SpellViewModel.Empty(SpellKeys.Q),
        SpellViewModel.Empty(SpellKeys.W),
        SpellViewModel.Empty(SpellKeys.E),
        SpellViewModel.Empty(SpellKeys.R),
    ];

    private List<SkinViewModel> _skins = [];
    private SkinViewModel? _selectedSkin;

    public IReadOnlyList<SpellViewModel> Spells => this._spells;
    public IReadOnlyList<SkinViewModel> Skins => this._skins;

    /// <summary>True si au moins un skin est disponible.</summary>
    public bool HasSkins => this._skins.Count > 0;

    /// <summary>Skin actuellement affiché au centre du carrousel.</summary>
    public SkinViewModel? SelectedSkin
    {
        get => this._selectedSkin;
        set
        {
            if (value == this._selectedSkin) return;
            this._selectedSkin = value;
            this.OnPropertyChanged(nameof(SelectedSkin));
            // Les aperçus dépendent du skin courant.
            this.OnPropertyChanged(nameof(PreviousSkin));
            this.OnPropertyChanged(nameof(NextSkin));
        }
    }

    /// <summary>Index du skin courant dans la liste (-1 si aucun).</summary>
    private int CurrentIndex =>
        this._selectedSkin is null ? -1 : this._skins.IndexOf(this._selectedSkin);

    /// <summary>Aperçu du skin précédent (boucle sur le dernier). Null si &lt; 2 skins.</summary>
    public SkinViewModel? PreviousSkin =>
        this._skins.Count <= 1 || this.CurrentIndex < 0
            ? null
            : this._skins[(this.CurrentIndex - 1 + this._skins.Count) % this._skins.Count];

    /// <summary>Aperçu du skin suivant (boucle sur le premier). Null si &lt; 2 skins.</summary>
    public SkinViewModel? NextSkin =>
        this._skins.Count <= 1 || this.CurrentIndex < 0
            ? null
            : this._skins[(this.CurrentIndex + 1) % this._skins.Count];

    // ── Stats par niveau ──────────────────────────────────────────────────

    /// <summary>Niveaux sélectionnables (1 à 18).</summary>
    public IReadOnlyList<int> Levels { get; } =
        Enumerable.Range(ChampionStatsCalculator.MinLevel, ChampionStatsCalculator.MaxLevel).ToList();

    private int _selectedLevel = ChampionStatsCalculator.MinLevel;

    /// <summary>Niveau choisi pour l'affichage des stats (1 à 18).</summary>
    public int SelectedLevel
    {
        get => this._selectedLevel;
        set
        {
            var clamped = Math.Clamp(value, ChampionStatsCalculator.MinLevel, ChampionStatsCalculator.MaxLevel);
            if (this._selectedLevel == clamped) return;
            this._selectedLevel = clamped;
            this.OnPropertyChanged(nameof(SelectedLevel));
            this.BuildStats();
        }
    }

    private IReadOnlyList<ChampionStatRow> _championStats = [];

    /// <summary>Stats du champion calculées pour <see cref="SelectedLevel"/>.</summary>
    public IReadOnlyList<ChampionStatRow> ChampionStats => this._championStats;

    // ── Objets équipés ────────────────────────────────────────────────────

    public const int MaxItems = 6;

    /// <summary>Tous les objets disponibles à l'équipement (recherche dans le sélecteur).</summary>
    public IReadOnlyList<ItemViewModel> AvailableItems => items.AllItems;

    /// <summary>Suggestions d'autocomplétion du sélecteur (réutilise celles des objets).</summary>
    public IReadOnlyList<ItemViewModel> EquipSuggestions => items.SearchSuggestions;

    /// <summary>Texte courant du sélecteur d'objet à équiper (lié à l'autocomplétion).</summary>
    public string EquipQuery
    {
        get;
        set { field = value ?? string.Empty; this.OnPropertyChanged(nameof(EquipQuery)); }
    } = string.Empty;

    /// <summary>Objets équipés sur le champion (max <see cref="MaxItems"/>).</summary>
    public ObservableCollection<ItemViewModel> EquippedItems { get; } = [];

    /// <summary>True s'il reste de la place pour équiper un objet.</summary>
    public bool CanEquipMore => this.EquippedItems.Count < MaxItems;

    /// <summary>Objet choisi dans le sélecteur, prêt à être équipé.</summary>
    public ItemViewModel? ItemToEquip
    {
        get;
        set { field = value; this.OnPropertyChanged(nameof(ItemToEquip)); }
    }

    /// <summary>Équipe <see cref="ItemToEquip"/> et recalcule les stats.</summary>
    public void EquipSelected()
    {
        if (this.ItemToEquip is null || !this.CanEquipMore) return;
        this.EquippedItems.Add(this.ItemToEquip);
        this.ItemToEquip = null;
        this.EquipQuery = string.Empty; // vide le sélecteur après équipement
        this.OnPropertyChanged(nameof(CanEquipMore));
        this.BuildStats();
    }

    /// <summary>Retire un objet équipé et recalcule les stats.</summary>
    public void Unequip(ItemViewModel item)
    {
        if (this.EquippedItems.Remove(item))
        {
            this.OnPropertyChanged(nameof(CanEquipMore));
            this.BuildStats();
        }
    }

    private Dictionary<string, double> AggregateEquippedStats()
    {
        var agg = new Dictionary<string, double>();
        foreach (var item in this.EquippedItems)
            foreach (var (key, value) in item.RawStats)
                agg[key] = (agg.TryGetValue(key, out var cur) ? cur : 0) + value;
        return agg;
    }

    private void BuildStats()
    {
        this._championStats = ChampionStatsCalculator
            .Compute(this.Champion?.Stats, this._selectedLevel, this.AggregateEquippedStats())
            .Select(s => new ChampionStatRow(LabelFor(s.Kind), FormatValue(s)))
            .ToList();
        this.OnPropertyChanged(nameof(ChampionStats));
    }

    private static string LabelFor(ChampionStatKind kind) => kind switch
    {
        ChampionStatKind.Health       => Resources.StatLabel_Health,
        ChampionStatKind.HealthRegen  => Resources.StatLabel_HealthRegen,
        ChampionStatKind.Mana         => Resources.StatLabel_Mana,
        ChampionStatKind.ManaRegen    => Resources.StatLabel_ManaRegen,
        ChampionStatKind.Armor        => Resources.StatLabel_Armor,
        ChampionStatKind.MagicResist  => Resources.StatLabel_MagicResist,
        ChampionStatKind.AttackDamage => Resources.StatLabel_AttackDamage,
        ChampionStatKind.AbilityPower => Resources.StatLabel_AbilityPower,
        ChampionStatKind.AttackSpeed  => Resources.StatLabel_AttackSpeed,
        ChampionStatKind.MoveSpeed    => Resources.StatLabel_MoveSpeed,
        ChampionStatKind.AttackRange  => Resources.StatLabel_AttackRange,
        ChampionStatKind.CritChance   => Resources.StatLabel_CritChance,
        ChampionStatKind.LifeSteal    => Resources.StatLabel_LifeSteal,
        _                             => kind.ToString(),
    };

    private static string FormatValue(ChampionStatValue s)
    {
        var c = CultureInfo.CurrentCulture;
        return s.Kind switch
        {
            ChampionStatKind.AttackSpeed => s.Value.ToString("0.###", c),
            // Crit et vol de vie sont des fractions (0.20) → affichées en pourcentage.
            ChampionStatKind.CritChance or ChampionStatKind.LifeSteal
                                         => (s.Value * 100).ToString("0.#", c) + " %",
            ChampionStatKind.Health or ChampionStatKind.Mana or ChampionStatKind.AbilityPower or
            ChampionStatKind.MoveSpeed or ChampionStatKind.AttackRange
                                         => Math.Round(s.Value).ToString("0", c),
            _                            => s.Value.ToString("0.#", c),
        };
    }

    // ── Chargement ────────────────────────────────────────────────────────

    public async Task LoadAsync()
    {
        logger.LogInformation("Chargement du champion '{Name}'", this.ChampionName);

        try
        {
            var riotTask    = httpRiot.GetChampionDetail(this.ChampionName);
            var cdragonSpellTask = cdragon.GetSpellCalculationsAsync(this.ChampionName);

            await Task.WhenAll(riotTask, cdragonSpellTask);

            this.Champion = riotTask.Result;
            var cdragonCalcs = cdragonSpellTask.Result;

            logger.LogInformation(
                "Champion '{Name}' chargé — {SpellCount} sort(s), {SkinCount} skin(s), {CalcSpellCount} sort(s) CDragon",
                this.Champion?.Name, this._spells.Count, this.Champion?.Skins?.Count ?? 0, cdragonCalcs.Count);

            this.BuildSpells(cdragonCalcs);

            logger.LogInformation("Construction des skins");
            this.BuildSkins();

            this.BuildStats();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du chargement du champion '{Name}'", this.ChampionName);
        }
    }

    private void BuildSkins()
    {
        this._skins = this.Champion?.Skins is { } skins
            // Exclut les chromas (variantes de couleur) : DataDragon les liste comme
            // des skins, mais ils portent un parentSkin et n'ont pas de splash propre.
            ? skins.Where(skin => skin.ParentSkin is null)
                   .Select(skin => new SkinViewModel
                   {
                       Num          = skin.Num ?? 0,
                       ChampionId   = this.ChampionName,
                       DisplayName  = DisplaySkinName(skin, this.Champion),
                       HasChromas   = skin.Chromas ?? false,
                   }).ToList()
            : [];

        // Affiche le premier skin (skin de base) par défaut.
        this.SelectedSkin = this._skins.FirstOrDefault();

        this.OnPropertyChanged(nameof(Skins));
        this.OnPropertyChanged(nameof(HasSkins));

        logger.LogDebug("{Count} skin(s) construit(s)", this._skins.Count);
    }

    /// <summary>
    /// Nom affiché d'un skin : le skin de base (num 0 / "default") prend le nom
    /// du champion ; les autres gardent leur nom Riot.
    /// </summary>
    private static string DisplaySkinName(Skin skin, Champion? champion)
    {
        var isBaseSkin = skin.Num is 0 or null
            || string.Equals(skin.Name, "default", StringComparison.OrdinalIgnoreCase);

        return isBaseSkin
            ? champion?.Name ?? string.Empty
            : skin.Name ?? string.Empty;
    }

    // ── Construction des sorts ────────────────────────────────────────────

    private void BuildSpells(Dictionary<string, Dictionary<string, SpellCalculation>>? cdragonCalcs = null)
    {
        this._spells = [];
        if (this.Champion is null) return;

        var normalizedName = this.Champion.Id ?? string.Empty;

        if (this.Champion.Passive is not null)
        {
            var passiveVm = SpellViewModel.FromPassive(this.Champion.Passive);

            // Le passif a souvent des calculs CDragon (ex : Garen RegenCalc).
            // Clé CDragon = "<Id>Passive" ; repli sur toute clé finissant par "Passive".
            var passiveCalcs = FindPassiveCalcs(cdragonCalcs, normalizedName);
            if (passiveCalcs is not null)
                passiveVm = passiveVm.WithFormulas(passiveCalcs);

            this._spells.Add(passiveVm);
            logger.LogDebug("Passif '{Name}' ajouté — {FormulaCount} formule(s) CDragon",
                this.Champion.Passive.Name, passiveVm.FormulaRows.Count);
        }

        string[] keys   = SpellKeys.Active;
        var spells = this.Champion.Spells ?? [];

        foreach (var (key, spell) in keys.Zip(spells))
        {
            var vm = SpellViewModel.FromSpell(key, spell);

            if (cdragonCalcs is not null)
            {
                var cdragonKey = $"{normalizedName}{key}";
                if (cdragonCalcs.TryGetValue(cdragonKey, out var calcs))
                {
                    vm = vm.WithFormulas(calcs);
                    logger.LogDebug("[CDragon] {FormulaCount} formule(s) pour {Key} ({CdragonKey})",
                        calcs.Count, key, cdragonKey);
                }
                else
                {
                    logger.LogDebug("[CDragon] Aucune formule trouvée pour {Key} ({CdragonKey})", key, cdragonKey);
                }
            }

            this._spells.Add(vm);
            logger.LogDebug("Sort {Key} '{Name}' ajouté — {RowCount} leveltip, {FormulaCount} formule(s) CDragon",
                key, spell.Name, vm.LeveltipRows.Count, vm.FormulaRows.Count);
        }

        this.OnPropertyChanged(nameof(Spells));
    }

    /// <summary>
    /// Retrouve les calculs CDragon du passif : essaie "&lt;Id&gt;Passive",
    /// puis n'importe quelle clé se terminant par "Passive".
    /// </summary>
    private static Dictionary<string, SpellCalculation>? FindPassiveCalcs(
        Dictionary<string, Dictionary<string, SpellCalculation>>? calcs, string normalizedName)
    {
        if (calcs is null) return null;

        if (calcs.TryGetValue($"{normalizedName}Passive", out var direct)) return direct;

        var key = calcs.Keys.FirstOrDefault(k => k.EndsWith("Passive", StringComparison.OrdinalIgnoreCase));
        return key is not null ? calcs[key] : null;
    }

    /// <summary>Passe au skin suivant ; revient au premier après le dernier (boucle).</summary>
    public void SelectNextSkin()
    {
        if (this._skins.Count <= 1) return;
        var nextIndex = (this.CurrentIndex + 1) % this._skins.Count;
        this.SelectedSkin = this._skins[nextIndex];
    }

    /// <summary>Passe au skin précédent ; revient au dernier avant le premier (boucle).</summary>
    public void SelectPreviousSkin()
    {
        if (this._skins.Count <= 1) return;
        var previousIndex = (this.CurrentIndex - 1 + this._skins.Count) % this._skins.Count;
        this.SelectedSkin = this._skins[previousIndex];
    }
}
