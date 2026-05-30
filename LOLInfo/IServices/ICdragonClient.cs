namespace LOLInfo.IServices;

using System.Collections.Generic;
using System.Threading.Tasks;

using LOLInfo.Models.CdragonModel;

/// <summary>
/// Contrat pour l'accès aux données CDragon raw (bin.json).
/// Retourne les formules de scaling d'un champion sous forme de
/// dictionnaire SpellId → (nom calcul → <see cref="SpellCalculation"/>).
/// </summary>
public interface ICdragonClient
{
    /// <summary>
    /// Récupère les calculs de tous les sorts d'un champion depuis CDragon.
    /// Retourne un dictionnaire vide en cas d'erreur réseau ou de champion inconnu.
    /// </summary>
    Task<Dictionary<string, Dictionary<string, SpellCalculation>>> GetSpellCalculationsAsync(string championName);
}
