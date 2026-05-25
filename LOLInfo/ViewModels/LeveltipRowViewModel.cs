namespace LOLInfo.ViewModels
{
    /// <summary>
    /// Représente une ligne du tableau "gain par niveau" d'un sort.
    ///
    /// L'API Riot stocke le leveltip comme deux listes parallèles :
    ///   Leveltip.Label  = ["Dégâts",         "Temps de recharge"]
    ///   Leveltip.Effect = ["60/95/130/165/200", "13/12/11/10/9"]
    ///
    /// Cette classe zippe les deux listes pour obtenir des paires bindables :
    ///   Label  = "Dégâts"
    ///   Effect = "60/95/130/165/200"
    ///
    /// La View itère sur une List&lt;LeveltipRowViewModel&gt; avec un ItemsControl,
    /// ce qui est impossible à faire directement sur deux listes séparées en XAML.
    /// </summary>
    public class LeveltipRowViewModel
    {
        /// <summary>
        /// Libellé de la statistique concernée.
        /// Exemple : "Dégâts", "Temps de recharge", "Coût", "Bonus AD".
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Valeurs aux niveaux 1-5 (ou 1-3 pour les ults), séparées par "/".
        /// Exemple : "60/95/130/165/200"
        /// </summary>
        public string Effect { get; }

        public LeveltipRowViewModel(string label, string effect)
        {
            Label  = label;
            Effect = effect;
        }
    }
}
