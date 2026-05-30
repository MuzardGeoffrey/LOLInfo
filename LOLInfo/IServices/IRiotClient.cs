namespace LOLInfo.IServices;

using System.Collections.ObjectModel;
using System.Threading.Tasks;

using LOLInfo.Models.RiotModel;

public interface IRiotClient
{
    Task<ObservableCollection<Champion>> GetAllChampions();
    Task<Champion> GetChampionDetail(string championName);
}
