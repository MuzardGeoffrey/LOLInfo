namespace LOLInfo.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LOLInfo.IServices;
using LOLInfo.IViewModels;
using LOLInfo.Models;
using LOLInfo.Models.CdragonModel;
using LOLInfo.Models.RiotModel;

using Microsoft.Extensions.Logging;

public class DetailChampionViewModel(
    IRiotClient httpRiot,
    ICdragonClient cdragon,
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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du chargement du champion '{Name}'", this.ChampionName);
        }
    }

    private void BuildSkins()
    {
        this._skins = this.Champion?.Skins is { } skins
            ? skins.Select(skin => new SkinViewModel
            {
                Num          = skin.Num ?? 0,
                ChampionId   = this.ChampionName,
                DisplayName  = DisplaySkinName(skin, this.Champion),
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
            this._spells.Add(SpellViewModel.FromPassive(this.Champion.Passive));
            logger.LogDebug("Passif '{Name}' ajouté", this.Champion.Passive.Name);
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
