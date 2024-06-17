namespace LOLInfo.IViewModels
{
    using System.Threading.Tasks;

    using LOLInfo.Models.RiotModel;

    public interface IDetailChampionPageViewModel
    {
        public Task<Champion> GetChampionDetail(string championName);
    }
}