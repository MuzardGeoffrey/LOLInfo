namespace LOLInfo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LOLInfo.IViewModels;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.Services;

    using Microsoft.Extensions.Logging;

    public class DetailChampionViewModel : BaseViewModel, IDetailChampionViewModel
    {
        private readonly IViewManager _viewManager;
        private readonly IRiotClient _httpRiot;
        private readonly ILogger<DetailChampionViewModel> _logger;

        // ── Identité ─────────────────────────────────────────────────────

        public string ChampionName { get; }

        // ── Champion chargé ───────────────────────────────────────────────

        private Champion? _champion;

        /// <summary>
        /// Null tant que LoadAsync() n'a pas terminé.
        /// Quand la propriété est définie, OnPropertyChanged notifie la View
        /// et BuildSpells() construit la liste des sorts.
        /// </summary>
        public Champion? Champion
        {
            get => _champion;
            private set
            {
                _champion = value;
                OnPropertyChanged(nameof(Champion));
                BuildSpells();
            }
        }

        // ── Sorts ─────────────────────────────────────────────────────────

        // Les 5 placeholders évitent les ArgumentOutOfRangeException quand WPF
        // évalue Spells[0..4] avant la fin de LoadAsync().
        // BuildSpells() les remplacera par les données réelles une fois chargées.
        private List<SpellViewModel> _spells = new()
        {
            SpellViewModel.Empty("Passif"),
            SpellViewModel.Empty("Q"),
            SpellViewModel.Empty("W"),
            SpellViewModel.Empty("E"),
            SpellViewModel.Empty("R"),
        };

        /// <summary>
        /// Passif + Q + W + E + R, dans l'ordre, prêts pour l'affichage.
        /// Initialisé avec des placeholders vides, puis remplacé par LoadAsync().
        /// </summary>
        public IReadOnlyList<SpellViewModel> Spells => _spells;

        // ── Constructeur ──────────────────────────────────────────────────

        public DetailChampionViewModel(
            IViewManager viewManager,
            IRiotClient httpRiot,
            string championName,
            ILogger<DetailChampionViewModel> logger)
        {
            _viewManager  = viewManager;
            _httpRiot     = httpRiot;
            _logger       = logger;
            ChampionName  = championName;
        }

        // ── Chargement ───────────────────────────────────────────────────

        /// <summary>
        /// Appelé depuis DetailChampionPage.Loaded.
        /// Charge les données du champion puis met à jour Champion,
        /// ce qui déclenche BuildSpells() via le setter.
        /// </summary>
        public async Task LoadAsync()
        {
            _logger.LogInformation("Chargement du champion '{Name}'", ChampionName);

            try
            {
                Champion = await _httpRiot.GetChampionDetail(ChampionName);

                _logger.LogInformation(
                    "Champion '{Name}' chargé — {SpellCount} sort(s), {SkinCount} skin(s)",
                    Champion?.Name, _spells.Count, Champion?.Skins?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du champion '{Name}'", ChampionName);
            }
        }

        // ── Construction des sorts ────────────────────────────────────────

        /// <summary>
        /// Construit la liste Passif + Q + W + E + R depuis les données brutes.
        /// Appelé automatiquement par le setter de Champion.
        /// </summary>
        private void BuildSpells()
        {
            _spells = new List<SpellViewModel>();

            if (_champion is null) return;

            // Passif
            if (_champion.Passive is not null)
            {
                _spells.Add(SpellViewModel.FromPassive(_champion.Passive));
                _logger.LogDebug("Passif '{Name}' ajouté", _champion.Passive.Name);
            }

            // Sorts actifs dans l'ordre Q/W/E/R
            var keys  = new[] { "Q", "W", "E", "R" };
            var spells = _champion.Spells ?? new List<Spell>();

            foreach (var (key, spell) in keys.Zip(spells))
            {
                _spells.Add(SpellViewModel.FromSpell(key, spell));
                _logger.LogDebug("Sort {Key} '{Name}' ajouté — {RowCount} ligne(s) leveltip",
                    key, spell.Name, SpellViewModel.FromSpell(key, spell).LeveltipRows.Count);
            }

            OnPropertyChanged(nameof(Spells));
        }
    }
}
