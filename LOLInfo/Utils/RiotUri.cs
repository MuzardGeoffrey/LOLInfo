namespace LOLInfo.Utils
{
    public static class RiotUri
    {
        private const string PathApiRiot = "https://ddragon.leagueoflegends.com/cdn/14.11.1/data/fr_FR/";

        private const string PathGeneral = "champion.json";

        public static string Detail(string championName) => $"{PathApiRiot}{championName}.json";

        public static string General() => $"{PathApiRiot}{PathGeneral}";
    }
}