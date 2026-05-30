namespace LOLInfo.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using LOLInfo.IServices;
using LOLInfo.Models.CdragonModel;
using LOLInfo.Utils;

using Microsoft.Extensions.Logging;

/// <summary>
/// Télécharge le bin.json CDragon du champion, extrait les SpellObjects
/// et les parse en <see cref="SpellCalculation"/>.
/// </summary>
public class CdragonClient(ILogger<CdragonClient> logger) : ICdragonClient
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };
    private const string BaseUrl = "https://raw.communitydragon.org/latest/game/data/characters";

    public async Task<Dictionary<string, Dictionary<string, SpellCalculation>>> GetSpellCalculationsAsync(
        string championName)
    {
        var result = new Dictionary<string, Dictionary<string, SpellCalculation>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(championName)) return result;

        var normalized = ChampionNameNormalizer.Normalize(championName);
        var url = $"{BaseUrl}/{normalized}/{normalized}.bin.json";

        logger.LogDebug("[CDragon] Téléchargement bin.json pour '{Champion}' — URL : {Url}", championName, url);

        try
        {
            var json = await _http.GetStringAsync(url);
            result = this.ParseBinJson(json, normalized);
            logger.LogInformation("[CDragon] {SpellCount} sorts parsés pour '{Champion}'", result.Count, championName);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "[CDragon] Erreur réseau pour '{Champion}' (URL : {Url})", championName, url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CDragon] Erreur inattendue pour '{Champion}' (URL : {Url})", championName, url);
        }

        return result;
    }

    private Dictionary<string, Dictionary<string, SpellCalculation>> ParseBinJson(
        string json, string normalizedName)
    {
        var result = new Dictionary<string, Dictionary<string, SpellCalculation>>(StringComparer.OrdinalIgnoreCase);

        JsonNode? root;
        try { root = JsonNode.Parse(json); }
        catch (JsonException ex)
        {
            logger.LogError(ex, "[CDragon] JSON invalide pour '{Champion}'", normalizedName);
            return result;
        }

        if (root is not JsonObject rootObj) return result;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var kvp in rootObj)
        {
            if (!kvp.Key.ToLowerInvariant().Contains("/spells/") || kvp.Value is null) continue;

            var spellId = kvp.Key.Split('/').LastOrDefault() ?? kvp.Key;

            try
            {
                var wrapper  = kvp.Value.Deserialize<CdragonSpellObjectRaw>(options);
                var spellRaw = wrapper?.SpellData ?? kvp.Value.Deserialize<CdragonSpellRaw>(options);

                if (spellRaw?.SpellCalculations is null) continue;

                var calcs = CdragonChampionParser.ParseCalculations(spellRaw, logger);
                if (calcs.Count > 0) result[spellId] = calcs;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "[CDragon] Impossible de parser le sort '{SpellId}'", spellId);
            }
        }

        return result;
    }
}
