namespace LOLInfo.IServices.Storage;

using System.Collections.Generic;

/// <summary>
/// Mémorise la langue choisie par l'utilisateur entre deux démarrages.
/// </summary>
public interface ILanguageService
{
    /// <summary>Code culture .NET actuellement retenu (ex : "en", "fr").</summary>
    string CurrentLanguage { get; }

    /// <summary>Langues proposées (codes culture .NET).</summary>
    IReadOnlyList<string> AvailableLanguages { get; }

    /// <summary>
    /// Définit et persiste la langue. Ignore une valeur non supportée.
    /// Le changement n'est pleinement effectif qu'au prochain démarrage
    /// (pas de bascule à chaud pour l'instant).
    /// </summary>
    void SetLanguage(string cultureName);
}
