namespace LOLInfo.Models.CdragonModel;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// ──────────────────────────────────────────────────────────────────────────────
// Modèles de désérialisation brute du fichier bin.json CDragon.
//
// Structure actuelle (patch 13+) :
//   "Characters/Ahri/Spells/AhriQAbility/AhriQ": {
//     "__type": "SpellObject",
//     "mSpell": { "DataValues": [...], "mSpellCalculations": { ... } }
//   }
//
// Structure ancienne (avant patch 13) :
//   "Characters/Ahri/Spells/AhriQ": {
//     "__type": "SpellObject",
//     "mEffectAmount": [...],
//     "mDataValues": [...],
//     "mSpellCalculations": { ... }
//   }
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Enveloppe d'un SpellObject dans le bin.json CDragon (structure actuelle).
/// Les données de formule sont dans <see cref="SpellData"/> (champ <c>mSpell</c>).
/// </summary>
public class CdragonSpellObjectRaw
{
    /// <summary>Données de sort imbriquées (patch 13+).</summary>
    [JsonPropertyName("mSpell")]
    public CdragonSpellRaw? SpellData { get; set; }
}

/// <summary>
/// Données brutes d'un sort CDragon (contenu de <c>mSpell</c> ou racine selon le patch).
/// </summary>
public class CdragonSpellRaw
{
    /// <summary>Tableau 2D de valeurs par rang (ancien format).</summary>
    [JsonPropertyName("mEffectAmount")]
    public List<List<double>>? EffectAmount { get; set; }

    /// <summary>Valeurs nommées par rang — ancien nom de champ (mDataValues).</summary>
    [JsonPropertyName("mDataValues")]
    public List<CdragonDataValue>? DataValuesLegacy { get; set; }

    /// <summary>Valeurs nommées par rang — nouveau nom de champ (DataValues, patch 13+).</summary>
    [JsonPropertyName("DataValues")]
    public List<CdragonDataValue>? DataValues { get; set; }

    /// <summary>Calculs de sorts : clé = nom du calcul, valeur = formule.</summary>
    [JsonPropertyName("mSpellCalculations")]
    public Dictionary<string, CdragonSpellCalculationRaw>? SpellCalculations { get; set; }

    /// <summary>
    /// Fusionne les DataValues des deux versions du format. En cas de conflit de nom,
    /// le nouveau format (<see cref="DataValues"/>) est prioritaire sur l'ancien
    /// (<see cref="DataValuesLegacy"/>).
    /// </summary>
    public List<CdragonDataValue> GetDataValues()
    {
        var merged = new List<CdragonDataValue>();
        var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dv in DataValues ?? [])
            if (dv.Name is not null && seen.Add(dv.Name))
                merged.Add(dv);

        foreach (var dv in DataValuesLegacy ?? [])
            if (dv.Name is not null && seen.Add(dv.Name))
                merged.Add(dv);

        return merged;
    }
}

/// <summary>
/// Une entrée DataValues dans le bin.json CDragon.
/// Structure réelle (CDragon actuel) :
///   { "name": "BaseDamage", "values": [0, 30, 60, 90, 120, 150, 180], "__type": "SpellDataValue" }
/// Les clés sont "name"/"values" (et NON "mName"/"mValues") — c'est ce mauvais
/// mapping qui faisait que toutes les valeurs ressortaient vides (formules « ? »).
/// </summary>
public class CdragonDataValue
{
    [JsonPropertyName("name")]   public string? Name   { get; set; }
    [JsonPropertyName("values")] public List<double>? Values { get; set; }
}

/// <summary>Un calcul de sort brut : liste de parties de formule.</summary>
public class CdragonSpellCalculationRaw
{
    [JsonPropertyName("mFormulaParts")]
    public List<CdragonFormulaPartRaw>? FormulaParts { get; set; }
}

/// <summary>
/// Un composant brut d'une formule CDragon.
/// Le champ <c>__type</c> détermine l'interprétation des autres champs.
/// </summary>
public class CdragonFormulaPartRaw
{
    [JsonPropertyName("__type")]          public string?  Type        { get; set; }
    [JsonPropertyName("mNumber")]         public double?  Number      { get; set; }
    [JsonPropertyName("mEffectIndex")]    public int?     EffectIndex { get; set; }
    [JsonPropertyName("mDataValue")]      public string?  DataValue   { get; set; }
    [JsonPropertyName("mStat")]           public int?     Stat        { get; set; }
    [JsonPropertyName("mStatFormula")]    public int?     StatFormula { get; set; }
    [JsonPropertyName("mCoefficient")]    public double?  Coefficient { get; set; }
    [JsonPropertyName("mLevel1Value")]    public double?  Level1Value { get; set; }
    [JsonPropertyName("mBreakpoints")]    public List<CdragonBreakpoint>? Breakpoints { get; set; }
    [JsonPropertyName("mStartValue")]     public double?  StartValue  { get; set; }
    [JsonPropertyName("mEndValue")]       public double?  EndValue    { get; set; }
    [JsonPropertyName("values")]          public List<double>? LevelValues { get; set; }
    [JsonPropertyName("mBuffName")]       public string?  BuffName    { get; set; }
    [JsonPropertyName("mSubParts")]       public List<CdragonFormulaPartRaw>? SubParts { get; set; }
    [JsonPropertyName("mSubpart")]        public CdragonFormulaPartRaw? SubPart { get; set; }
}

/// <summary>
/// Un palier de niveau dans <c>ByCharLevelBreakpointsCalculationPart</c>.
/// </summary>
public class CdragonBreakpoint
{
    [JsonPropertyName("mLevel")]                     public int    Level           { get; set; }
    [JsonPropertyName("mAdditionalBonusAtThisLevel")] public double AdditionalBonus { get; set; }
}
