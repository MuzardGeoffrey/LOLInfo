namespace LOLInfo.ViewModels;

/// <summary>
/// Une option de tri de la liste d'objets : sépare la <see cref="Key"/> stable
/// (utilisée par <see cref="ItemsViewModel"/> pour choisir le comparateur) du
/// <see cref="Label"/> traduit affiché dans la ComboBox.
///
/// Pour un tri par statistique, <see cref="Key"/> est la clé brute DataDragon
/// (ex : "FlatPhysicalDamageMod") ; les tris génériques utilisent les clés
/// spéciales "name", "cost_asc" et "cost_desc".
/// </summary>
public sealed class ItemSortOption(string key, string label)
{
    public string Key   { get; } = key;
    public string Label { get; } = label;

    /// <summary>Affiché par la ComboBox (DisplayMemberPath="Label").</summary>
    public override string ToString() => this.Label;
}
