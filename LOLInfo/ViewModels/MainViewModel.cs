namespace LOLInfo.ViewModels;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

using LOLInfo.IServices;
using LOLInfo.IServices.Storage;
using LOLInfo.IViewModels;
using LOLInfo.Localization;

public class MainViewModel : BaseViewModel
{
    private readonly IViewManager _viewManager;
    private readonly ILanguageService _language;

    public MainViewModel(IViewManager viewManager, IItemsViewModel items, ILanguageService language)
    {
        this._viewManager = viewManager;
        this.Items = items;
        this._language = language;
        this._viewManager.PropertyChanged += this.OnViewManagerPropertyChanged;

        this.Languages = language.AvailableLanguages
            .Select(code => new LanguageOption(code, AppLocalization.NativeName(code)))
            .ToList();
    }

    public Page? CurrentPage => this._viewManager.CurrentPage;

    /// <summary>ViewModel de l'onglet Objets.</summary>
    public IItemsViewModel Items { get; }

    /// <summary>Langues proposées dans le sélecteur (drapeau + endonyme).</summary>
    public IReadOnlyList<LanguageOption> Languages { get; }

    /// <summary>Code de la langue active (présélection du sélecteur).</summary>
    public string CurrentLanguageCode => this._language.CurrentLanguage;

    private void OnViewManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IViewManager.CurrentPage))
            this.OnPropertyChanged(nameof(CurrentPage));
    }
}
