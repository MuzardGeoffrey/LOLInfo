namespace LOLInfo.Models.CdragonModel;

using System.Collections.Generic;
using System.Linq;

using LOLInfo.Models.CdragonModel.Parts;

using Microsoft.Extensions.Logging;

/// <summary>
/// Construit un dictionnaire de <see cref="SpellCalculation"/> depuis
/// les données brutes <see cref="CdragonSpellRaw"/> d'un sort CDragon.
/// </summary>
public static class CdragonChampionParser
{
    /// <summary>
    /// Parse toutes les formules d'un sort CDragon.
    /// Retourne un dictionnaire vide si <paramref name="raw"/> est null ou sans calculs.
    /// </summary>
    public static Dictionary<string, SpellCalculation> ParseCalculations(
        CdragonSpellRaw? raw,
        ILogger? logger = null)
    {
        var result = new Dictionary<string, SpellCalculation>(StringComparer.OrdinalIgnoreCase);

        if (raw?.SpellCalculations is null) return result;

        var effectAmount = BuildEffectAmount(raw.EffectAmount);
        var dataValues   = BuildDataValues(raw.GetDataValues());

        foreach (var (calcName, calcRaw) in raw.SpellCalculations)
        {
            if (calcRaw?.FormulaParts is null) continue;

            var parts = calcRaw.FormulaParts
                .Select(p => BuildPart(p, effectAmount, dataValues, logger))
                .Where(p => p is not null)
                .Cast<IFormulaPart>()
                .ToList();

            result[calcName] = new SpellCalculation(calcName, parts);
        }

        return result;
    }

    // ── Helpers de construction ───────────────────────────────────────────

    private static IFormulaPart? BuildPart(
        CdragonFormulaPartRaw raw,
        IReadOnlyList<IReadOnlyList<double>> effectAmount,
        IReadOnlyDictionary<string, IReadOnlyList<double>> dataValues,
        ILogger? logger) => raw.Type switch
    {
        "NumberCalculationPart" =>
            new NumberPart(raw.Number ?? 0),

        "EffectValueCalculationPart" =>
            raw.EffectIndex.HasValue
                ? new EffectValuePart(raw.EffectIndex.Value, effectAmount)
                : null,

        "NamedDataValueCalculationPart" =>
            raw.DataValue is not null
                ? new NamedDataValuePart(raw.DataValue, dataValues)
                : null,

        "StatByNamedDataValueCalculationPart" =>
            raw.Stat.HasValue && raw.DataValue is not null
                ? new StatScalingPart(raw.Stat.Value, raw.StatFormula ?? 2, raw.DataValue, dataValues)
                : null,

        "StatByCoefficientCalculationPart" =>
            new StatByCoefficientPart(raw.Coefficient ?? 0, raw.Stat ?? -1, raw.StatFormula ?? 2),

        "ByCharLevelBreakpointsCalculationPart" =>
            new ByLevelBreakpointsPart(
                raw.Level1Value ?? 0,
                raw.Breakpoints?.Select(b => (b.Level, b.AdditionalBonus)).ToList() ?? []),

        "ByCharLevelInterpolationCalculationPart" =>
            new ByLevelInterpolationPart(raw.StartValue ?? 0, raw.EndValue ?? 0),

        "ByCharLevelFormulaCalculationPart" =>
            new ByLevelFormulaPart(raw.LevelValues ?? []),

        "BuffCounterByNamedDataValueCalculationPart" =>
            raw.DataValue is not null
                ? new BuffCounterDataValuePart(
                    dataValues.TryGetValue(raw.DataValue, out var bv) ? bv : [])
                : null,

        "BuffCounterByCoefficientCalculationPart" =>
            new BuffCounterCoefficientPart(raw.Coefficient ?? 0),

        "AbilityResourceByCoefficientCalculationPart" =>
            new AbilityResourceCoefficientPart(raw.Coefficient ?? 0),

        "ProductOfSubPartsCalculationPart" =>
            new ProductOfSubPartsPart(BuildSubParts(raw.SubParts, effectAmount, dataValues, logger)),

        "SumOfSubPartsCalculationPart" =>
            new SumOfSubPartsPart(BuildSubParts(raw.SubParts, effectAmount, dataValues, logger)),

        "StatBySubPartCalculationPart" =>
            raw.SubPart is not null
                ? new StatBySubPartPart(
                    (ChampionStat)(raw.Stat ?? -1),
                    raw.StatFormula ?? 2,
                    BuildPart(raw.SubPart, effectAmount, dataValues, logger) ?? new UnknownPart("null"))
                : null,

        "ClampSubPartsCalculationPart" =>
            new ClampSubPartsPart(BuildSubParts(raw.SubParts, effectAmount, dataValues, logger)),

        _ => LogUnknown(raw.Type, logger),
    };

    private static IReadOnlyList<IFormulaPart> BuildSubParts(
        List<CdragonFormulaPartRaw>? rawList,
        IReadOnlyList<IReadOnlyList<double>> effectAmount,
        IReadOnlyDictionary<string, IReadOnlyList<double>> dataValues,
        ILogger? logger)
    {
        if (rawList is null) return [];
        return [.. rawList
            .Select(p => BuildPart(p, effectAmount, dataValues, logger))
            .Where(p => p is not null)
            .Cast<IFormulaPart>()];
    }

    private static UnknownPart LogUnknown(string? typeName, ILogger? logger)
    {
        logger?.LogDebug("[CdragonChampionParser] Type de formulaPart inconnu : '{Type}'", typeName);
        return new UnknownPart(typeName);
    }

    private static IReadOnlyList<IReadOnlyList<double>> BuildEffectAmount(List<List<double>>? raw)
    {
        if (raw is null) return [];
        return [.. raw.Select(inner => (IReadOnlyList<double>)(inner?.AsReadOnly() ?? (IReadOnlyList<double>)[]))];
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<double>> BuildDataValues(List<CdragonDataValue> raw)
    {
        var dict = new Dictionary<string, IReadOnlyList<double>>(StringComparer.OrdinalIgnoreCase);
        foreach (var dv in raw)
        {
            if (dv.Name is null || dv.Values is null) continue;
            dict[dv.Name] = dv.Values.AsReadOnly();
        }
        return dict;
    }
}
