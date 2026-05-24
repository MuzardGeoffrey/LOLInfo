namespace LOLInfo.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using LOLInfo.Models.RiotModel;

    public interface IRiotClient
    {
        Task<ObservableCollection<Champion>> GetAllChampions();
        Task<Champion> GetChampionDetail(string championName);
    }
}