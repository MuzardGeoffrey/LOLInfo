namespace LOLInfo.Services
{
    using System.ComponentModel;
    using System.Windows.Controls;

    public interface IViewManager : INotifyPropertyChanged
    {
        Page CurrentPage { get; }
        void NavigateToAllChampion();
        void NavigateToDetail(string championName);
    }
}