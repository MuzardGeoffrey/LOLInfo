namespace LOLInfo.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class Champion()
    {
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("blurb")]
        public string? Blurb { get; set; }

        [JsonPropertyName("info")]
        public Info? Info { get; set; }

        [JsonPropertyName("image")]
        public Image? Image { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("partype")]
        public string? Partype { get; set; }

        [JsonPropertyName("stats")]
        public Dictionary<string, double>? Stats { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("skins")]
        public Skin[]? Skins { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("lore")]
        public string? Lore { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("allytips")]
        public List<string>? Allytips { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("enemytips")]
        public List<string>? Enemytips { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("spells")]
        public Spell[]? Spells { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("passive")]
        public Passive? Passive { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("recommended")]
        public List<object>? Recommended { get; set; }
    }
}