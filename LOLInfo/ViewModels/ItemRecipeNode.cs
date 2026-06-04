namespace LOLInfo.ViewModels;

using System.Collections.Generic;

/// <summary>
/// Nœud de l'arbre de fabrication (recette) d'un objet.
///
/// La racine est l'objet final ; ses <see cref="Components"/> sont les composants
/// directs (champ <c>from</c> de DataDragon), résolus récursivement jusqu'aux
/// objets de base. C'est la structure liée par <c>RecipeTreeNode.xaml</c> pour
/// dessiner l'arbre descendant (objet en haut, composants en dessous).
/// </summary>
public sealed class ItemRecipeNode
{
    /// <summary>Profondeur maximale explorée (garde-fou contre des données circulaires).</summary>
    private const int MaxDepth = 8;

    public required string Id        { get; init; }
    public required string Name      { get; init; }
    public required string ImagePath { get; init; }
    public int             Gold      { get; init; }

    public IReadOnlyList<ItemRecipeNode> Components { get; init; } = [];

    public bool HasComponents => this.Components.Count > 0;

    // ── Position dans la fratrie : pilote l'affichage de la barre de connexion ──
    // Premier enfant → pas de demi-barre à gauche ; dernier → pas de demi-barre à droite.
    public bool IsFirst { get; set; }
    public bool IsLast  { get; set; }

    public bool ShowLeftBar  => !this.IsFirst;
    public bool ShowRightBar => !this.IsLast;

    /// <summary>
    /// Construit l'arbre de fabrication de <paramref name="item"/> en résolvant
    /// ses composants via <paramref name="byId"/>. Les identifiants introuvables
    /// (objet filtré ou absent) sont simplement ignorés.
    /// </summary>
    public static ItemRecipeNode Build(ItemViewModel item, IReadOnlyDictionary<string, ItemViewModel> byId)
        => Build(item, byId, MaxDepth);

    private static ItemRecipeNode Build(ItemViewModel item, IReadOnlyDictionary<string, ItemViewModel> byId, int depth)
    {
        var children = new List<ItemRecipeNode>();
        if (depth > 0)
        {
            foreach (var componentId in item.ComponentIds)
            {
                if (byId.TryGetValue(componentId, out var component))
                    children.Add(Build(component, byId, depth - 1));
            }
        }

        for (var i = 0; i < children.Count; i++)
        {
            children[i].IsFirst = i == 0;
            children[i].IsLast  = i == children.Count - 1;
        }

        return new ItemRecipeNode
        {
            Id         = item.Id,
            Name       = item.Name,
            ImagePath  = item.ImagePath,
            Gold       = item.Gold,
            Components = children,
        };
    }
}
