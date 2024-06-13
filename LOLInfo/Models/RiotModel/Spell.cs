namespace LOLInfo.Models.RiotModel;

using System.Text.Json.Serialization;

public class Spell
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("tooltip")]
    public string? Tooltip { get; set; }

    [JsonPropertyName("leveltip")]
    public Leveltip? Leveltip { get; set; }

    [JsonPropertyName("maxrank")]
    public int? Maxrank { get; set; }

    [JsonPropertyName("cooldown")]
    public List<int>? Cooldown { get; set; }

    [JsonPropertyName("cooldownBurn")]
    public string? CooldownBurn { get; set; }

    [JsonPropertyName("cost")]
    public List<int>? Cost { get; set; }

    [JsonPropertyName("costBurn")]
    public string? CostBurn { get; set; }

    [JsonPropertyName("datavalues")]
    public Datavalues? Datavalues { get; set; }

    [JsonPropertyName("effect")]
    public List<int>? Effect { get; set; }

    [JsonPropertyName("effectBurn")]
    public List<string>? EffectBurn { get; set; }

    [JsonPropertyName("vars")]
    public List<object>? Vars { get; set; }

    [JsonPropertyName("costType")]
    public string? CostType { get; set; }

    [JsonPropertyName("maxammo")]
    public string? Maxammo { get; set; }

    [JsonPropertyName("range")]
    public List<int>? Range { get; set; }

    [JsonPropertyName("rangeBurn")]
    public string? RangeBurn { get; set; }

    [JsonPropertyName("image")]
    public Image? Image { get; set; }

    [JsonPropertyName("resource")]
    public string? Resource { get; set; }
}