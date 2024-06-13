namespace LOLInfo.IViewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;

using LOLInfo.Models.RiotModel;

public interface IMainViewModel
{
    public Task<List<Champion>> GetAllChampions();
}