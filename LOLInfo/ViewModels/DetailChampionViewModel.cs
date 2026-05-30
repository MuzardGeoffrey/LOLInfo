namespace LOLInfo.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LOLInfo.IServices;
using LOLInfo.IViewModels;
using LOLInfo.Models.CdragonModel;
using LOLInfo.Models.RiotModel;

using Microsoft.Extensions.Logging;

public class DetailChampionViewModel(
    IViewManager viewManager,
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
        SpellViewModel.Empty("Passif"),
        SpellViewModel.Empty("Q"),
        SpellViewModel.Empty("W"),
        SpellViewModel.Empty("E"),
        SpellViewModel.Empty("R"),
    ];

    public IReadOnlyList<SpellViewModel> Spells => this._spells;

    // ── Chargement ────────────────────────────────────────────────────────

    public async Task LoadAsync()
    {
        logger.LogInformation("Chargement du champion '{Name}'", this.ChampionName);

        try
        {
            var riotTask    = httpRiot.GetChampionDetail(this.ChampionName);
            var cdragonTask = cdragon.GetSpellCalculationsAsync(this.ChampionName);

            await Task.WhenAll(riotTask, cdragonTask);

            this.Champion = riotTask.Result;
            var cdragonCalcs = cdragonTask.Result;

            logger.LogInformation(
                "Champion '{Name}' chargé — {SpellCount} sort(s), {SkinCount} skin(s), {CalcSpellCount} sort(s) CDragon",
                this.Champion?.Name, this._spells.Count, this.Champion?.Skins?.Count ?? 0, cdragonCalcs.Count);

            this.BuildSpells(cdragonCalcs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du chargement du champion '{Name}'", this.ChampionName);
        }
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

        string[] keys   = ["Q", "W", "E", "R"];
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
}
