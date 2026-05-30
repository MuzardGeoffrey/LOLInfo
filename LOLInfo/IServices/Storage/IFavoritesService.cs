namespace LOLInfo.IServices.Storage;

using System.Collections.Generic;

public interface IFavoritesService
{
    bool IsFavorite(string? championId);
    bool Toggle(string championId);
    IReadOnlyCollection<string> GetAll();
}
