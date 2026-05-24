namespace LOLInfo.ViewModels
{
    using LOLInfo.IViewModels;
    using LOLInfo.Services;

    using Models.RiotModel;

    public class DetailChampionViewModel : BaseViewModel, IDetailChampionViewModel
    {
        private readonly IViewManager _viewManager;

        public string ChampionName { get; set; }

        /// <summary>
        /// Wraps l'appel HTTP async et notifie la vue quand le résultat arrive.
        /// Le XAML bind sur ChampionSelected.Result.*
        /// </summary>
        public NotifyTaskCompletion<Champion> ChampionSelected { get; private set; }

        public DetailChampionViewModel(IViewManager viewManager, IRiotClient httpRiot, string championName)
        {
            _viewManager = viewManager;
            ChampionName = championName;
            ChampionSelected = new NotifyTaskCompletion<Champion>(httpRiot.GetChampionDetail(championName));
        }
    }
}