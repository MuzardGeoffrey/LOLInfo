namespace LOLInfo.Services;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using LOLInfo.IServices;
using LOLInfo.Models.RiotModel;
using LOLInfo.Utils;

using Microsoft.Extensions.Logging;

public class RiotClient(ILogger<RiotClient> logger) : IRiotClient
{
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(30) };

    public async Task<ObservableCollection<Champion>> GetAllChampions()
    {
        var url = RiotUri.GENERAL();
        logger.LogDebug("Récupération de la liste des champions — URL : {Url}", url);

        try
        {
            var response = await _client.GetFromJsonAsync<JsonRiotFormat>(url);
            var champions = response?.ChampionsList?.Values.ToList();

            if (champions is null || champions.Count == 0)
            {
                logger.LogWarning("La réponse de l'API ne contient aucun champion (URL : {Url})", url);
                return [];
            }

            logger.LogInformation("{Count} champions chargés depuis l'API", champions.Count);
            return new ObservableCollection<Champion>(champions);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Erreur réseau lors de la récupération de la liste des champions (URL : {Url})", url);
            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur inattendue lors de la récupération de la liste des champions (URL : {Url})", url);
            return [];
        }
    }

    public async Task<Champion> GetChampionDetail(string championName)
    {
        var url = RiotUri.DETAIL(championName);
        logger.LogDebug("Récupération du détail du champion '{ChampionName}' — URL : {Url}", championName, url);

        try
        {
            var response = await _client.GetFromJsonAsync<JsonRiotFormat>(url);
            var champion = response?.ChampionsList?.Values.FirstOrDefault();

            if (champion is null)
            {
                logger.LogWarning("Aucune donnée retournée pour '{ChampionName}' (URL : {Url})", championName, url);
                return new Champion();
            }

            logger.LogInformation("Détail chargé pour '{ChampionName}' — {SpellCount} sorts, {SkinCount} skins",
                champion.Name, champion.Spells?.Count ?? 0, champion.Skins?.Count ?? 0);

            return champion;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Erreur réseau pour '{ChampionName}' (URL : {Url})", championName, url);
            return new Champion();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur inattendue pour '{ChampionName}' (URL : {Url})", championName, url);
            return new Champion();
        }
    }
}
