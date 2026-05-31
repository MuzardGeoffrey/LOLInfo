namespace LOLInfo.Models.RiotModel;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>Un objet (item) de l'API DataDragon (item.json).</summary>
public class Item
{
    /// <summary>Identifiant de l'objet (clé du dictionnaire item.json, ex : "3031").
    /// Injecté au parsing — absent du JSON de la valeur.</summary>
    [JsonIgnore]
    public string? Id { get; set; }

    [JsonPropertyName("name")]        public string? Name        { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("plaintext")]   public string? Plaintext   { get; set; }
    [JsonPropertyName("image")]       public Image?  Image       { get; set; }
    [JsonPropertyName("gold")]        public ItemGold? Gold      { get; set; }
    [JsonPropertyName("tags")]        public List<string>? Tags  { get; set; }

    /// <summary>Stats brutes (ex : "FlatPhysicalDamageMod" → 70, "PercentAttackSpeedMod" → 0.25).</summary>
    [JsonPropertyName("stats")]       public Dictionary<string, double>? Stats { get; set; }

    /// <summary>Disponibilité par carte (ex : "11" = Faille, "12" = ARAM).</summary>
    [JsonPropertyName("maps")]        public Dictionary<string, bool>? Maps { get; set; }
}

/// <summary>Coût d'un objet.</summary>
public class ItemGold
{
    [JsonPropertyName("total")]       public int  Total       { get; set; }
    [JsonPropertyName("base")]        public int  Base        { get; set; }
    [JsonPropertyName("sell")]        public int  Sell        { get; set; }
    [JsonPropertyName("purchasable")] public bool Purchasable { get; set; }
}
