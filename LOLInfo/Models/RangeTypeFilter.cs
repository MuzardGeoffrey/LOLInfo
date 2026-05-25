namespace LOLInfo.Models
{
    /// <summary>
    /// Filtre sur le type de portée d'un champion.
    /// Dérivé de Champion.Stats["attackrange"] :
    ///   - Melee  : attackrange &lt;  300
    ///   - Range  : attackrange >= 300
    /// </summary>
    public enum RangeTypeFilter
    {
        Tous,
        Melee,
        Range,
    }
}
