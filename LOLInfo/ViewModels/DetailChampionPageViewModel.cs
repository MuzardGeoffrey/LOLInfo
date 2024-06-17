namespace LOLInfo.ViewModels
{
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using LOLInfo.IViewModels;
    using LOLInfo.Models.RiotModel;
    using LOLInfo.Utils;

    public class DetailChampionPageViewModel : IDetailChampionPageViewModel
    {
        private readonly HttpClient _client = new();

        public async Task<Champion> GetChampionDetail(string championName)
        {
            var response = await _client.GetFromJsonAsync<JsonRiotFormat>(RiotUri.Detail(championName));

            var champion = response?.ChampionsList?.Values.FirstOrDefault() ?? new Champion();

            return champion;
        }
    }
}