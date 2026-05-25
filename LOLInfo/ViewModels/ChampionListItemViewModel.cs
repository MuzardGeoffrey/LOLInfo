namespace LOLInfo.ViewModels
{
    using CommunityToolkit.Mvvm.Input;

    using LOLInfo.Models;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.Services.Storage;

    /// <summary>
    /// Wrapper MVVM d'un Champion pour la liste de sélection.
    /// Encapsule l'état favori et la commande de bascule,
    /// évitant tout binding RelativeSource complexe dans le DataTemplate.
    /// </summary>
    public class ChampionListItemViewModel : BaseViewModel
    {
        private readonly IFavoritesService _favoritesService;

        // ── Données du champion ───────────────────────────────────────────

        /// <summary>Champion brut issu de l'API Riot.</summary>
        public Champion Champion { get; }

        // ── Propriétés calculées (filtrables, calculées une seule fois) ───

        /// <summary>
        /// Type de dégâts principal, dérivé des scores Info.Attack / Info.Magic (0-10).
        /// Écart &gt;= 3 en faveur de l'un → AD ou AP, sinon → Mixte.
        /// </summary>
        public DamageTypeFilter DamageType { get; }

        /// <summary>
        /// Vrai si le champion est à distance (attackrange >= 300).
        /// Faux s'il est mêlée ou si la stat est absente.
        /// </summary>
        public bool IsRanged { get; }

        // ── Favori ────────────────────────────────────────────────────────

        private bool _isFavorite;

        /// <summary>
        /// Indique si ce champion est en favori.
        /// Mis à jour par ToggleFavoriteCommand et chargé depuis FavoritesService.
        /// </summary>
        public bool IsFavorite
        {
            get => _isFavorite;
            private set
            {
                _isFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
            }
        }

        /// <summary>
        /// Commande locale : bascule le statut favori et persiste via FavoritesService.
        /// </summary>
        public IRelayCommand ToggleFavoriteCommand { get; }

        // ── Constructeur ─────────────────────────────────────────────────

        public ChampionListItemViewModel(Champion champion, IFavoritesService favoritesService)
        {
            Champion = champion;
            _favoritesService = favoritesService;

            // Charge l'état initial depuis le service (lecture du JSON).
            _isFavorite = favoritesService.IsFavorite(champion.Id);

            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);

            // Calculs dérivés : effectués une seule fois à la construction,
            // mis en cache comme propriétés en lecture seule.
            DamageType = ComputeDamageType(champion);
            IsRanged   = ComputeIsRanged(champion);
        }

        // ── Logique privée ───────────────────────────────────────────────

        private void ToggleFavorite()
        {
            IsFavorite = _favoritesService.Toggle(Champion.Id);
        }

        /// <summary>
        /// Dérive le type de dégâts principal depuis les scores Info.Attack et Info.Magic.
        /// Les scores vont de 0 à 10. Un écart de 3 ou plus indique une dominance claire.
        /// Exemples : Caitlyn (9/0) → AD ; Lux (2/9) → AP ; Jax (7/4) → Mixte.
        /// </summary>
        private static DamageTypeFilter ComputeDamageType(Champion c)
        {
            var atk = c.Info?.Attack ?? 0;
            var mag = c.Info?.Magic  ?? 0;

            if (atk - mag >= 3) return DamageTypeFilter.AD;
            if (mag - atk >= 3) return DamageTypeFilter.AP;
            return DamageTypeFilter.Mixte;
        }

        /// <summary>
        /// Dérive le type de portée depuis Stats["attackrange"].
        /// Mêlée : portée &lt; 300 (ex : Garen = 175).
        /// Distance : portée >= 300 (ex : Caitlyn = 650, Lux = 550).
        /// </summary>
        private static bool ComputeIsRanged(Champion c)
        {
            if (c.Stats is null) return false;
            return c.Stats.TryGetValue("attackrange", out var range) && range >= 300;
        }
    }
}
