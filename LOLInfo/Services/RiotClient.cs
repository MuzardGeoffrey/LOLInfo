namespace LOLInfo.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;

    using LOLInfo.Models.RiotModel;
    using LOLInfo.Utils;

    public class RiotClient : IRiotClient
    {
        private HttpClient _client = new HttpClient();

        public async Task<Champion> GetChampionDetail(string championName)
        {
            var response = await _client.GetFromJsonAsync<JsonRiotFormat>(RiotUri.DETAIL(championName));

            return response?.ChampionsList?.Values.FirstOrDefault() ?? new Champion();
        }

        public async Task<ObservableCollection<Champion>> GetAllChampions()
        {
            var response = await this._client.GetFromJsonAsync<JsonRiotFormat>(RiotUri.GENERAL());

            var champions = response?.ChampionsList?.Values.ToList();

            var observableChampion = new ObservableCollection<Champion>();
            for (var i = 0; i < champions?.Count; i++)
            {
                var champion = champions[i];
                observableChampion.Add(champion);
            }

            return observableChampion;
        }
    }
}