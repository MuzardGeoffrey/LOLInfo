namespace LOLInfo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LOLInfo.Models.RiotModel;

    /// <summary>
    /// Wrapper bindable pour un sort (actif ou passif).
    ///
    /// Unifie Passive et Spell dans un seul type pour éviter la duplication
    /// de DataTemplate en XAML. Les propriétés absentes du passif sont vides.
    ///
    /// Utiliser les fabriques statiques :
    ///   SpellViewModel.FromPassive(passive)
    ///   SpellViewModel.FromSpell("Q", spell)
    ///   SpellViewModel.Empty("Q")        ← placeholder avant chargement
    /// </summary>
    public class SpellViewModel
    {
        // ── Identité ─────────────────────────────────────────────────────

        /// <summary>
        /// Touche d'activation : "Passif", "Q", "W", "E", "R".
        /// Affiché comme en-tête de l'onglet.
        /// </summary>
        public string Key { get; }

        /// <summary>Nom du sort (ex : "Orbe de la Décimation").</summary>
        public string Name { get; }

        /// <summary>Description courte du sort.</summary>
        public string Description { get; }

        /// <summary>
        /// Nom du fichier image (ex : "AhriQ.png").
        /// Passé à ImagePathConverter avec le bon ConverterParameter.
        /// </summary>
        public string IconPath { get; }

        /// <summary>True si ce sort est le passif du champion.</summary>
        public bool IsPassive { get; }

        // ── Tableau de progression par niveau ────────────────────────────

        /// <summary>
        /// En-têtes de colonnes du tableau de progression.
        ///
        /// Calculé depuis la première ligne de StatRows.
        ///
        /// Exemples :
        ///   Sort à 5 rangs (Q/W/E) → ["Niv 1", "Niv 2", "Niv 3", "Niv 4", "Niv 5"]
        ///   Sort à 3 rangs (R)      → ["Niv 1", "Niv 2", "Niv 3"]
        ///   Valeur constante        → ["Niv 1"]
        /// </summary>
        public IReadOnlyList<string> LevelHeaders =>
            StatRows.Count > 0
                ? Enumerable.Range(1, StatRows[0].Values.Count)
                            .Select(i => $"Niv {i}")
                            .ToList<string>()
                : Array.Empty<string>();

        /// <summary>
        /// Lignes du tableau de progression : recharge / coût / portée.
        ///
        /// Chaque SpellStatRow contient un label et une valeur par rang.
        /// Les lignes avec une valeur nulle ou "0" sont filtrées.
        /// Vide pour le passif (pas de stats) ou les placeholders Empty.
        /// </summary>
        public IReadOnlyList<SpellStatRow> StatRows { get; }

        // ── Gain par niveau (leveltip) ────────────────────────────────────

        /// <summary>
        /// Lignes leveltip filtrées : ne contient que les lignes dont l'effect
        /// est une valeur lisible (ex : "60/95/130/165/200").
        ///
        /// Les lignes avec des variables Riot non résolues
        /// (ex : "{{ basedamage }} -> {{ basedamageNL }}") sont exclues car
        /// l'API statique ne fournit pas les valeurs calculées.
        /// </summary>
        public IReadOnlyList<LeveltipRowViewModel> LeveltipRows { get; }

        // ── Helpers de visibilité ─────────────────────────────────────────

        /// <summary>
        /// True si le tableau de progression a au moins une ligne.
        /// Utilisé pour afficher / masquer le bloc Stats.
        /// </summary>
        public bool HasStats => StatRows.Count > 0;

        /// <summary>
        /// True si au moins une ligne de leveltip lisible est disponible.
        /// </summary>
        public bool HasLeveltip => LeveltipRows.Count > 0;

        // ── Constructeur privé ────────────────────────────────────────────

        private SpellViewModel(
            string key,
            string name,
            string description,
            string iconPath,
            bool isPassive,
            IReadOnlyList<SpellStatRow> statRows,
            IReadOnlyList<LeveltipRowViewModel> leveltipRows)
        {
            Key          = key;
            Name         = name         ?? string.Empty;
            Description  = description  ?? string.Empty;
            IconPath     = iconPath     ?? string.Empty;
            IsPassive    = isPassive;
            StatRows     = statRows;
            LeveltipRows = leveltipRows;
        }

        // ── Fabriques statiques ───────────────────────────────────────────

        /// <summary>
        /// Placeholder vide utilisé avant que les données ne soient chargées.
        /// Évite les ArgumentOutOfRangeException sur Spells[0..4] au premier
        /// rendu WPF (avant la fin de LoadAsync).
        /// </summary>
        public static SpellViewModel Empty(string key) =>
            new SpellViewModel(
                key:          key,
                name:         string.Empty,
                description:  string.Empty,
                iconPath:     string.Empty,
                isPassive:    false,
                statRows:     Array.Empty<SpellStatRow>(),
                leveltipRows: Array.Empty<LeveltipRowViewModel>()
            );

        /// <summary>
        /// Crée un SpellViewModel depuis le passif d'un champion.
        /// Pas de cooldown, coût, portée ni leveltip pour le passif.
        /// </summary>
        public static SpellViewModel FromPassive(Passive passive) =>
            new SpellViewModel(
                key:          "Passif",
                name:         passive.Name        ?? string.Empty,
                description:  passive.Description ?? string.Empty,
                iconPath:     passive.Image?.Full  ?? string.Empty,
                isPassive:    true,
                statRows:     Array.Empty<SpellStatRow>(),
                leveltipRows: Array.Empty<LeveltipRowViewModel>()
            );

        /// <summary>
        /// Crée un SpellViewModel depuis un sort actif.
        /// </summary>
        /// <param name="key">"Q", "W", "E" ou "R".</param>
        /// <param name="spell">Données brutes de l'API Riot.</param>
        public static SpellViewModel FromSpell(string key, Spell spell) =>
            new SpellViewModel(
                key:          key,
                name:         spell.Name        ?? string.Empty,
                description:  spell.Description ?? string.Empty,
                iconPath:     spell.Image?.Full  ?? string.Empty,
                isPassive:    false,
                statRows:     BuildStatRows(spell),
                leveltipRows: BuildLeveltipRows(spell)
            );

        // ── Construction interne ──────────────────────────────────────────

        /// <summary>
        /// Construit le tableau de progression depuis les BurnStrings de l'API.
        ///
        /// Riot fournit trois chaînes formatées "valeur/valeur/valeur" :
        ///   CooldownBurn : "9/8/7/6/5"  → une valeur par rang
        ///   CostBurn     : "55/65/75/85/95"
        ///   RangeBurn    : "970"         → constante sur tous les rangs
        ///
        /// On filtre les valeurs nulles ou "0" (sort sans ressource / portée self).
        /// </summary>
        private static IReadOnlyList<SpellStatRow> BuildStatRows(Spell spell)
        {
            var rows = new List<SpellStatRow>();

            // Recharge
            if (!string.IsNullOrWhiteSpace(spell.CooldownBurn))
                rows.Add(new SpellStatRow("⏱ Recharge", spell.CooldownBurn));

            // Coût (filtrer "0" = sort sans ressource, ex : Garen)
            if (!string.IsNullOrWhiteSpace(spell.CostBurn) && spell.CostBurn != "0")
                rows.Add(new SpellStatRow("💧 Coût", spell.CostBurn));

            // Portée (filtrer "0" = sort sans portée définie, ex : certains passifs)
            if (!string.IsNullOrWhiteSpace(spell.RangeBurn) && spell.RangeBurn != "0")
                rows.Add(new SpellStatRow("🎯 Portée", spell.RangeBurn));

            return rows;
        }

        /// <summary>
        /// Zippe Label et Effect du leveltip Riot en lignes lisibles.
        ///
        /// L'API Riot retourne deux listes parallèles :
        ///   Label  = ["Dégâts",              "Délai de récupération"]
        ///   Effect = ["{{ basedamage }}...",  "{{ cooldown }}..."]
        ///
        /// Les valeurs de type template (contenant "{{") ne sont pas résolues
        /// dans l'API statique — elles sont filtrées ici.
        /// Seules les lignes avec des valeurs numériques lisibles sont conservées,
        /// ex : ["60/95/130/165/200", "13/12/11/10/9"].
        /// </summary>
        private static IReadOnlyList<LeveltipRowViewModel> BuildLeveltipRows(Spell spell)
        {
            var labels  = spell.Leveltip?.Label;
            var effects = spell.Leveltip?.Effect;

            if (labels is null || effects is null || labels.Count == 0)
                return Array.Empty<LeveltipRowViewModel>();

            return labels
                .Zip(effects, (label, effect) => new LeveltipRowViewModel(label, effect))
                .Where(row => !row.Effect.Contains("{{"))
                .ToList();
        }
    }
}
