namespace LOLInfo.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Runtime.CompilerServices;

    using IViewModels;

    using Models.RiotModel;

    using Utils;

    internal class MainViewModel : IMainViewModel
    {
        private readonly HttpClient _client = new();

        public async Task<List<Champion>> GetAllChampions()
        {
            var response = await this._client.GetFromJsonAsync<JsonRiotFormat>(RiotUri.General());

            var champions = response?.ChampionsList?.Values.ToList();

            return champions ?? [];
        }
    }
}