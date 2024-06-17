namespace LOLInfo.IViewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;

using LOLInfo.Models.RiotModel;

public interface IAllChampionPageViewModel
{
    public Task<List<Champion>> GetAllChampions();
}