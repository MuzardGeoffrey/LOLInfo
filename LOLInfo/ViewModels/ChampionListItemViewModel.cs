namespace LOLInfo.ViewModels;

using CommunityToolkit.Mvvm.Input;

using LOLInfo.IServices.Storage;
using LOLInfo.Models;
using LOLInfo.Models.RiotModel;

/// <summary>
/// Wrapper MVVM d'un Champion pour la liste de sélection.
/// Encapsule l'état favori et la commande de bascule.
/// </summary>
public class ChampionListItemViewModel(Champion champion, IFavoritesService favoritesService) : BaseViewModel
{
    // ── Données du champion ───────────────────────────────────────────────

    public Champion Champion { get; } = champion;

    // ── Propriétés calculées (une seule fois à la construction) ──────────

    public DamageTypeFilter DamageType { get; } = ComputeDamageType(champion);
    public bool             IsRanged   { get; } = ComputeIsRanged(champion);

    // ── Favori (C# 14 : field keyword) ───────────────────────────────────

    /// <summary>
    /// Indique si ce champion est en favori.
    /// Initialisé depuis FavoritesService ; mis à jour par ToggleFavoriteCommand.
    /// </summary>
    public bool IsFavorite
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            this.OnPropertyChanged(nameof(IsFavorite));
        }
    } = favoritesService.IsFavorite(champion.Id);

    // ── Commande (lazy init — ne peut pas référencer this dans un initialiseur) ──

    private IRelayCommand? _toggleFavoriteCommand;

    /// <summary>Bascule le statut favori et persiste via FavoritesService.</summary>
    public IRelayCommand ToggleFavoriteCommand =>
        this._toggleFavoriteCommand ??= new RelayCommand(this.ToggleFavorite);

    // ── Logique ───────────────────────────────────────────────────────────

    private void ToggleFavorite()
    {
        if (this.Champion.Id is null) return;
        this.IsFavorite = favoritesService.Toggle(this.Champion.Id);
    }

    private static DamageTypeFilter ComputeDamageType(Champion c)
    {
        var atk = c.Info?.Attack ?? 0;
        var mag = c.Info?.Magic  ?? 0;
        if (atk - mag >= 3) return DamageTypeFilter.AD;
        if (mag - atk >= 3) return DamageTypeFilter.AP;
        return DamageTypeFilter.Mixte;
    }

    private static bool ComputeIsRanged(Champion c) =>
        c.Stats is not null &&
        c.Stats.TryGetValue("attackrange", out var range) &&
        range >= 300;
}
