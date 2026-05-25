namespace LOLInfo.Services
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using LOLInfo.Models.RiotModel;
    using LOLInfo.Utils;

    using Microsoft.Extensions.Logging;

    public class RiotClient : IRiotClient
    {
        private readonly HttpClient _client = new();
        private readonly ILogger<RiotClient> _logger;

        public RiotClient(ILogger<RiotClient> logger)
        {
            _logger = logger;
        }

        // ── Liste de tous les champions ───────────────────────────────────

        public async Task<ObservableCollection<Champion>> GetAllChampions()
        {
            var url = RiotUri.GENERAL();
            _logger.LogDebug("Récupération de la liste des champions — URL : {Url}", url);

            try
            {
                var response = await _client.GetFromJsonAsync<JsonRiotFormat>(url);
                var champions = response?.ChampionsList?.Values.ToList();

                if (champions is null || champions.Count == 0)
                {
                    _logger.LogWarning("La réponse de l'API ne contient aucun champion (URL : {Url})", url);
                    return new ObservableCollection<Champion>();
                }

                _logger.LogInformation("{Count} champions chargés depuis l'API", champions.Count);
                return new ObservableCollection<Champion>(champions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur réseau lors de la récupération de la liste des champions (URL : {Url})", url);
                return new ObservableCollection<Champion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la récupération de la liste des champions (URL : {Url})", url);
                return new ObservableCollection<Champion>();
            }
        }

        // ── Détail d'un champion ──────────────────────────────────────────

        public async Task<Champion> GetChampionDetail(string championName)
        {
            var url = RiotUri.DETAIL(championName);
            _logger.LogDebug("Récupération du détail du champion '{ChampionName}' — URL : {Url}", championName, url);

            try
            {
                var response = await _client.GetFromJsonAsync<JsonRiotFormat>(url);
                var champion = response?.ChampionsList?.Values.FirstOrDefault();

                if (champion is null)
                {
                    _logger.LogWarning("Aucune donnée retournée pour le champion '{ChampionName}' (URL : {Url})", championName, url);
                    return new Champion();
                }

                _logger.LogInformation("Détail chargé pour '{ChampionName}' — {SpellCount} sorts, {SkinCount} skins",
                    champion.Name,
                    champion.Spells?.Count ?? 0,
                    champion.Skins?.Count ?? 0);

                return champion;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur réseau lors de la récupération du champion '{ChampionName}' (URL : {Url})", championName, url);
                return new Champion();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la récupération du champion '{ChampionName}' (URL : {Url})", championName, url);
                return new Champion();
            }
        }
    }
}
