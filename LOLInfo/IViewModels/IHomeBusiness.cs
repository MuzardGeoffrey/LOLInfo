namespace LOLInfo.IViewModels;

using LOLInfo.Models;

public interface IHomeBusiness
{
    public static Task<List<Champion>> GenerateHomePages();

    public static Task<Champion> GetChampionDetail(string championName);
}