namespace LOLInfo.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    /// Base commune pour tous les ViewModels.
    /// ObservableObject fournit déjà INotifyPropertyChanged et OnPropertyChanged().
    /// </summary>
    public class BaseViewModel : ObservableObject
    {
    }
}