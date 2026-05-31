namespace LOLInfo.IServices;

using System.Collections.Generic;
using System.Threading.Tasks;

using LOLInfo.Models.RiotModel;

/// <summary>Récupère les objets (items) depuis l'API DataDragon.</summary>
public interface IItemClient
{
    /// <summary>
    /// Retourne les objets dotés de statistiques (hors trinkets), triés par coût puis nom.
    /// </summary>
    Task<IReadOnlyList<Item>> GetAllItems();
}
