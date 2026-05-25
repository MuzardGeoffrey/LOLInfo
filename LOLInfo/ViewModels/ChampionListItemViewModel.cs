namespace LOLInfo.ViewModels
{
    using CommunityToolkit.Mvvm.Input;

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
        }

        // ── Logique privée ───────────────────────────────────────────────

        private void ToggleFavorite()
        {
            IsFavorite = _favoritesService.Toggle(Champion.Id);
        }
    }
}
