namespace LOLInfo.Models
{
    /// <summary>
    /// Filtre sur le type de dégâts principal d'un champion.
    /// Dérivé de Champion.Info.Attack vs Champion.Info.Magic (scores 0-10).
    /// </summary>
    public enum DamageTypeFilter
    {
        Tous,
        AD,
        AP,
        Mixte,
    }
}
