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

    // double et non int : certaines valeurs sont décimales (ex : Akali Q = 1.5,
    // Akali E = 14.5). Un int ferait échouer toute la désérialisation du champion.
    [JsonPropertyName("cooldown")]
    public List<double>? Cooldown { get; set; }

    [JsonPropertyName("cooldownBurn")]
    public string? CooldownBurn { get; set; }

    [JsonPropertyName("cost")]
    public List<double>? Cost { get; set; }

    [JsonPropertyName("costBurn")]
    public string? CostBurn { get; set; }

    [JsonPropertyName("datavalues")]
    public Datavalues? Datavalues { get; set; }

    // Structure réelle de l'API Riot :
    //   effect[0]   = null          (placeholder, le tableau est 1-indexé)
    //   effect[1..] = [v1,v2,v3,v4,v5]  (valeurs par niveau pour chaque composante)
    // → List<List<int>?>  : liste dont le premier élément peut être null,
    //   les suivants étant des listes d'entiers.
    [JsonPropertyName("effect")]
    public List<List<double>?>? Effect { get; set; }

    // effectBurn[0] = null  (même convention que effect)
    // effectBurn[1..] = "valeur" (chaîne formatée par niveau)
    // → List<string?> : les éléments peuvent être null.
    [JsonPropertyName("effectBurn")]
    public List<string?>? EffectBurn { get; set; }

    [JsonPropertyName("vars")]
    public List<object>? Vars { get; set; }

    [JsonPropertyName("costType")]
    public string? CostType { get; set; }

    [JsonPropertyName("maxammo")]
    public string? Maxammo { get; set; }

    [JsonPropertyName("range")]
    public List<double>? Range { get; set; }

    [JsonPropertyName("rangeBurn")]
    public string? RangeBurn { get; set; }

    [JsonPropertyName("image")]
    public Image? Image { get; set; }

    [JsonPropertyName("resource")]
    public string? Resource { get; set; }
}