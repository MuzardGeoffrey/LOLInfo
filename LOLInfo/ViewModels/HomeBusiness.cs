namespace LOLInfo.ViewModels
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;

    using IViewModels;

    using Models;

    using Utils;

    internal class HomeBusiness : IHomeBusiness
    {
        public HomeBusiness()
        {
        }

        public static async Task<List<Champion>> GenerateHomePages()
        {
            var client = new HttpClient();
            var response = await client.GetFromJsonAsync<List<Champion>>(RiotUri.General()) ?? [];

            return response;
        }

        public static async Task<Champion> GetChampionDetail(string championName)
        {
            var client = new HttpClient();
            var response = await client.GetFromJsonAsync<Champion>(RiotUri.Detail(championName)) ?? new Champion();

            return response;
        }
    }
}