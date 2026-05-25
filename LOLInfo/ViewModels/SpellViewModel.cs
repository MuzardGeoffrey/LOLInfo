namespace LOLInfo.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using LOLInfo.Models.RiotModel;

    /// <summary>
    /// Wrapper bindable pour un sort (actif ou passif).
    ///
    /// Unifie Passive et Spell dans un seul type pour éviter la duplication
    /// de DataTemplate en XAML. Les propriétés absentes du passif sont null.
    ///
    /// Utiliser les fabriques statiques :
    ///   SpellViewModel.FromPassive(passive)
    ///   SpellViewModel.FromSpell("Q", spell)
    /// </summary>
    public class SpellViewModel
    {
        // ── Identité ─────────────────────────────────────────────────────

        /// <summary>
        /// Touche d'activation : "Passif", "Q", "W", "E", "R".
        /// Affiché comme en-tête de l'onglet et dans le badge.
        /// </summary>
        public string Key { get; }

        /// <summary>Nom du sort (ex : "Orbe de la Décimation").</summary>
        public string Name { get; }

        /// <summary>Description courte du sort.</summary>
        public string Description { get; }

        /// <summary>
        /// Nom du fichier image (ex : "AhriOrbofDeception.png").
        /// Passé au convertisseur ImagePathConverter avec le bon ConverterParameter.
        /// </summary>
        public string IconPath { get; }

        /// <summary>
        /// True = passif → le convertisseur utilise le dossier "passive/".
        /// False = sort actif → le convertisseur utilise le dossier "spell/".
        /// </summary>
        public bool IsPassive { get; }

        // ── Stats (sorts actifs uniquement) ──────────────────────────────

        /// <summary>
        /// Temps de recharge à chaque niveau, format "12/10/8/6/4".
        /// Null pour le passif.
        /// </summary>
        public string? CooldownBurn { get; }

        /// <summary>
        /// Coût en ressource à chaque niveau, format "50/60/70/80/90".
        /// Null pour le passif.
        /// </summary>
        public string? CostBurn { get; }

        /// <summary>
        /// Portée à chaque niveau, format "750" ou "700/750/800/850/900".
        /// Null pour le passif ou si non applicable.
        /// </summary>
        public string? RangeBurn { get; }

        // ── Gain par niveau ───────────────────────────────────────────────

        /// <summary>
        /// Tableau des gains par niveau, construit depuis Spell.Leveltip.
        /// Chaque ligne est une paire (label, valeurs par niveau).
        /// Vide pour le passif ou si l'API ne fournit pas de leveltip.
        /// </summary>
        public IReadOnlyList<LeveltipRowViewModel> LeveltipRows { get; }

        // ── Helpers de visibilité ─────────────────────────────────────────

        /// <summary>
        /// True si au moins une stat (cooldown ou coût) est disponible.
        /// Utilisé pour afficher / masquer le panneau Stats dans la View.
        /// </summary>
        public bool HasStats =>
            !string.IsNullOrWhiteSpace(CooldownBurn) ||
            !string.IsNullOrWhiteSpace(CostBurn);

        /// <summary>
        /// True si au moins une ligne de leveltip est disponible.
        /// Utilisé pour afficher / masquer le tableau Gain par niveau.
        /// </summary>
        public bool HasLeveltip => LeveltipRows.Count > 0;

        // ── Constructeur privé ────────────────────────────────────────────

        private SpellViewModel(
            string key,
            string name,
            string description,
            string iconPath,
            bool isPassive,
            string? cooldownBurn,
            string? costBurn,
            string? rangeBurn,
            IReadOnlyList<LeveltipRowViewModel> leveltipRows)
        {
            Key          = key;
            Name         = name          ?? string.Empty;
            Description  = description   ?? string.Empty;
            IconPath     = iconPath      ?? string.Empty;
            IsPassive    = isPassive;
            CooldownBurn = cooldownBurn;
            CostBurn     = costBurn;
            RangeBurn    = rangeBurn;
            LeveltipRows = leveltipRows;
        }

        // ── Fabriques statiques ───────────────────────────────────────────

        /// <summary>
        /// Crée un SpellViewModel vide (placeholder avant que les données ne soient chargées).
        /// Permet d'initialiser la liste Spells avec 5 éléments dès le constructeur du ViewModel,
        /// évitant ainsi les ArgumentOutOfRangeException lors des premiers bindings WPF.
        /// </summary>
        public static SpellViewModel Empty(string key) =>
            new SpellViewModel(
                key:          key,
                name:         string.Empty,
                description:  string.Empty,
                iconPath:     string.Empty,
                isPassive:    false,
                cooldownBurn: null,
                costBurn:     null,
                rangeBurn:    null,
                leveltipRows: new List<LeveltipRowViewModel>()
            );

        /// <summary>
        /// Crée un SpellViewModel depuis un passif.
        /// Pas de cooldown, coût, portée ni leveltip.
        /// </summary>
        public static SpellViewModel FromPassive(Passive passive)
        {
            return new SpellViewModel(
                key:          "Passif",
                name:         passive.Name        ?? string.Empty,
                description:  passive.Description  ?? string.Empty,
                iconPath:     passive.Image?.Full  ?? string.Empty,
                isPassive:    true,
                cooldownBurn: null,
                costBurn:     null,
                rangeBurn:    null,
                leveltipRows: new List<LeveltipRowViewModel>()
            );
        }

        /// <summary>
        /// Crée un SpellViewModel depuis un sort actif.
        /// </summary>
        /// <param name="key">"Q", "W", "E" ou "R".</param>
        /// <param name="spell">Données brutes de l'API Riot.</param>
        public static SpellViewModel FromSpell(string key, Spell spell)
        {
            return new SpellViewModel(
                key:          key,
                name:         spell.Name          ?? string.Empty,
                description:  spell.Description   ?? string.Empty,
                iconPath:     spell.Image?.Full    ?? string.Empty,
                isPassive:    false,
                cooldownBurn: spell.CooldownBurn,
                costBurn:     spell.CostBurn,
                rangeBurn:    spell.RangeBurn,
                leveltipRows: BuildLeveltipRows(spell)
            );
        }

        // ── Construction du leveltip ──────────────────────────────────────

        /// <summary>
        /// Zippe les deux listes parallèles Label et Effect du leveltip.
        /// Si l'une des deux est absente ou vide, retourne une liste vide.
        ///
        /// Exemple :
        ///   Label  = ["Dégâts",             "Temps de recharge"]
        ///   Effect = ["60/95/130/165/200",  "13/12/11/10/9"]
        ///   →
        ///   Row[0] : Label="Dégâts",            Effect="60/95/130/165/200"
        ///   Row[1] : Label="Temps de recharge", Effect="13/12/11/10/9"
        /// </summary>
        private static IReadOnlyList<LeveltipRowViewModel> BuildLeveltipRows(Spell spell)
        {
            var labels  = spell.Leveltip?.Label;
            var effects = spell.Leveltip?.Effect;

            if (labels is null || effects is null || labels.Count == 0)
                return new List<LeveltipRowViewModel>();

            // Zip protège contre les listes de longueurs différentes.
            // On filtre les lignes dont l'effect contient des variables Riot non résolues
            // (ex : "{{ basedamage }} -> {{ basedamageNL }}") : ces chaînes sont du
            // template interne de Riot et n'ont aucune valeur lisible pour l'utilisateur.
            return labels
                .Zip(effects, (label, effect) => new LeveltipRowViewModel(label, effect))
                .Where(row => !row.Effect.Contains("{{"))
                .ToList();
        }
    }
}
