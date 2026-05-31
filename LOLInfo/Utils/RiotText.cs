namespace LOLInfo.Utils
{
    using System.Net;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Nettoyage du texte fourni par l'API Riot (descriptions de sorts).
    /// </summary>
    public static class RiotText
    {
        // <br>, <br/>, <br />, <BR> … → un vrai saut de ligne.
        private static readonly Regex BrTag =
            new(@"<\s*br\s*/?\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Frontières de mots en camel/PascalCase : "TotalDamage" → "Total Damage".
        private static readonly Regex CamelBoundary =
            new(@"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled);

        private static readonly Regex ListItemTag = new(@"<\s*li\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AnyTag      = new(@"<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex BlankLines  = new(@"\n{3,}", RegexOptions.Compiled);

        /// <summary>
        /// Remplace les balises &lt;br&gt; par des sauts de ligne propres et
        /// retire les espaces superflus. Null/vide → chaîne vide.
        /// </summary>
        public static string CleanDescription(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return BrTag.Replace(text, "\n").Trim();
        }

        /// <summary>
        /// Rend lisible un identifiant camel/PascalCase : "TotalDamage" → "Total Damage",
        /// "RegenCalc" → "Regen Calc". Première lettre en majuscule. Null/vide → vide.
        /// </summary>
        public static string Humanize(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            var spaced = CamelBoundary.Replace(name, " ");
            return char.ToUpperInvariant(spaced[0]) + spaced[1..];
        }

        /// <summary>
        /// Convertit une description HTML Riot (objets) en texte lisible : &lt;br&gt; et
        /// &lt;li&gt; deviennent des sauts de ligne, toutes les autres balises sont retirées,
        /// et les entités HTML sont décodées. Null/vide → vide.
        /// </summary>
        public static string StripHtml(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var s = BrTag.Replace(text, "\n");
            s = ListItemTag.Replace(s, "\n• ");
            s = AnyTag.Replace(s, string.Empty);
            s = WebUtility.HtmlDecode(s);
            s = BlankLines.Replace(s, "\n\n");
            return s.Trim();
        }
    }
}
