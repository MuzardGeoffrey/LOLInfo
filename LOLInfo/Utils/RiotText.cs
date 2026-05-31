namespace LOLInfo.Utils
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Nettoyage du texte fourni par l'API Riot (descriptions de sorts).
    /// </summary>
    public static class RiotText
    {
        // <br>, <br/>, <br />, <BR> … → un vrai saut de ligne.
        private static readonly Regex BrTag =
            new(@"<\s*br\s*/?\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Remplace les balises &lt;br&gt; par des sauts de ligne propres et
        /// retire les espaces superflus. Null/vide → chaîne vide.
        /// </summary>
        public static string CleanDescription(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return BrTag.Replace(text, "\n").Trim();
        }
    }
}
