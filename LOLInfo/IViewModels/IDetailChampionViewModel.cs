namespace LOLInfo.IViewModels
{
    using LOLInfo.Models.RiotModel;
    using LOLInfo.Services;

    public interface IDetailChampionViewModel
    {
        string ChampionName { get; set; }
        NotifyTaskCompletion<Champion> ChampionSelected { get; }
    }
}