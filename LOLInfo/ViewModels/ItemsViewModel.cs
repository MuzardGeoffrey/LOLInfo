namespace LOLInfo.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

using LOLInfo.IServices;
using LOLInfo.IViewModels;

using Microsoft.Extensions.Logging;

/// <summary>
/// Liste des objets avec recherche par nom et sélection (panneau de détail).
/// Mirroir simplifié d'<see cref="AllChampionViewModel"/>.
/// </summary>
public class ItemsViewModel(IItemClient client, ILogger<ItemsViewModel> logger) : BaseViewModel, IItemsViewModel
{
    private List<ItemViewModel> _items = [];

    private ICollectionView _itemsView =
        CollectionViewSource.GetDefaultView(new List<ItemViewModel>());

    public ICollectionView ItemsView
    {
        get => this._itemsView;
        private set { this._itemsView = value; this.OnPropertyChanged(nameof(ItemsView)); }
    }

    public bool IsLoaded { get; private set; }

    public string NameFilter
    {
        get;
        set
        {
            field = value ?? string.Empty;
            this.OnPropertyChanged(nameof(NameFilter));
            this.ItemsView?.Refresh();
        }
    } = string.Empty;

    public ItemViewModel? SelectedItem
    {
        get;
        set { field = value; this.OnPropertyChanged(nameof(SelectedItem)); }
    }

    public async Task LoadAsync()
    {
        if (this.IsLoaded) return;

        logger.LogDebug("Chargement des objets");
        var items = await client.GetAllItems();

        this._items = items.Select(ItemViewModel.From).ToList();

        this.ItemsView = CollectionViewSource.GetDefaultView(this._items);
        this.ItemsView.Filter = this.ApplyFilter;
        this.SelectedItem = this._items.FirstOrDefault();
        this.IsLoaded = true;

        logger.LogInformation("Objets chargés — {Count} affiché(s)", this._items.Count);
    }

    private bool ApplyFilter(object obj)
    {
        if (obj is not ItemViewModel item) return false;
        if (string.IsNullOrEmpty(this.NameFilter)) return true;
        return item.Name.Contains(this.NameFilter, StringComparison.OrdinalIgnoreCase);
    }
}
