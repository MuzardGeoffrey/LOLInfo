namespace LOLInfo.Models;

/// <summary>
/// Mode de jeu (carte) sur lequel filtrer les objets.
/// La correspondance vers l'identifiant de carte DataDragon (champ <c>maps</c>)
/// est faite dans <see cref="ViewModels.ItemsViewModel"/>.
/// </summary>
public enum GameModeFilter { Tous, NormalClasse, Aram, Arena }
