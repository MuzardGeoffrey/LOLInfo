namespace LOLInfo.Services.Storage
{
    using System.Collections.Generic;

    public interface IFavoritesService
    {
        /// <summary>Indique si le champion est en favori.</summary>
        bool IsFavorite(string championId);

        /// <summary>
        /// Bascule le statut favori du champion.
        /// Retourne true si le champion est maintenant en favori, false sinon.
        /// </summary>
        bool Toggle(string championId);

        /// <summary>Retourne tous les IDs de champions en favoris.</summary>
        IReadOnlyCollection<string> GetAll();
    }
}
