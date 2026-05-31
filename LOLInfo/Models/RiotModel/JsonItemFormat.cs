namespace LOLInfo.Models.RiotModel;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>Enveloppe du fichier item.json de DataDragon (data = dictionnaire id → objet).</summary>
public class JsonItemFormat
{
    [JsonPropertyName("type")]    public string? Type    { get; set; }
    [JsonPropertyName("version")] public string? Version { get; set; }
    [JsonPropertyName("data")]    public Dictionary<string, Item>? Items { get; set; }
}
