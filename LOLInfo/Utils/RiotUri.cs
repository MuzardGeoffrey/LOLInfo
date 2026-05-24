namespace LOLInfo.Utils
{
    public static class RiotUri
    {
        private const string PathApiRiot = "https://ddragon.leagueoflegends.com/cdn/14.11.1/data/fr_FR/";

        private const string PathGeneral = "champion.json";

        public static string DETAIL(string championName) => $"{PathApiRiot}champion/{championName}.json";

        public static string GENERAL() => $"{PathApiRiot}{PathGeneral}";
    }
}