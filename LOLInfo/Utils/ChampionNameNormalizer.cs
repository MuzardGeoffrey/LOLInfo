namespace LOLInfo.Utils
{
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Normalise un nom de champion DataDragon en identifiant CDragon.
    ///
    /// CDragon attend le nom en minuscules, sans espaces, apostrophes ni
    /// caractères spéciaux.
    ///
    /// Exemples :
    ///   "Ahri"       → "ahri"
    ///   "Jarvan IV"  → "jarvaniv"
    ///   "Kai'Sa"     → "kaisa"
    ///   "Nunu &amp; Willump" → "nunuwillump"
    ///   "Renata Glasc" → "renataglasc"
    /// </summary>
    public static class ChampionNameNormalizer
    {
        private static readonly Regex _stripNonAlpha = new(@"[^a-z0-9]", RegexOptions.Compiled);

        /// <summary>
        /// Retourne l'identifiant CDragon correspondant au nom DataDragon donné.
        /// </summary>
        public static string Normalize(string championName)
        {
            if (string.IsNullOrWhiteSpace(championName))
                return string.Empty;

            // 1. Minuscules
            string lower = championName.ToLowerInvariant();

            // 2. Supprime les accents (NFD → ASCII)
            string normalized = RemoveDiacritics(lower);

            // 3. Supprime tout ce qui n'est pas a-z ou 0-9 (espaces, apostrophes, &, .)
            return _stripNonAlpha.Replace(normalized, string.Empty);
        }

        /// <summary>
        /// Décompose les caractères accentués et conserve uniquement les lettres ASCII.
        /// Ex : "è" → "e", "ü" → "u".
        /// </summary>
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalizedString.Length);

            foreach (char c in normalizedString)
            {
                var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
