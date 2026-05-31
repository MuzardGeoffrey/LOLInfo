namespace LOLInfo.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using LOLInfo.IServices;
using LOLInfo.Models.RiotModel;
using LOLInfo.Utils;

using Microsoft.Extensions.Logging;

/// <summary>
/// Télécharge item.json et retourne les objets dotés de stats, hors trinkets.
/// </summary>
public class ItemClient(ILogger<ItemClient> logger) : IItemClient
{
    private const string TrinketTag = "Trinket";
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public async Task<IReadOnlyList<Item>> GetAllItems()
    {
        var url = DataDragonCdn.ItemDataUrl();
        logger.LogDebug("Récupération des objets — URL : {Url}", url);

        try
        {
            var response = await _http.GetFromJsonAsync<JsonItemFormat>(url);
            var data = response?.Items;

            if (data is null || data.Count == 0)
            {
                logger.LogWarning("item.json ne contient aucun objet (URL : {Url})", url);
                return [];
            }

            var items = new List<Item>();
            foreach (var (id, item) in data)
            {
                if (item is null) continue;
                item.Id = id;

                // On garde les objets nommés, dotés de stats, et on exclut les trinkets.
                if (string.IsNullOrWhiteSpace(item.Name)) continue;
                if (item.Stats is null || item.Stats.Count == 0) continue;
                if (item.Tags is not null && item.Tags.Contains(TrinketTag)) continue;

                items.Add(item);
            }

            var ordered = items
                .OrderBy(i => i.Gold?.Total ?? 0)
                .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            logger.LogInformation("{Count} objet(s) avec stats chargé(s) (hors trinkets)", ordered.Count);
            return ordered;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Erreur réseau lors de la récupération des objets (URL : {Url})", url);
            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur inattendue lors de la récupération des objets (URL : {Url})", url);
            return [];
        }
    }
}
