namespace LOLInfo.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class JsonRiotFormat
    {
        [JsonPropertyName("type")]
        public TypeEnum Type { get; set; }

        [JsonPropertyName("format")]
        public string? Format { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, Champion>? ChampionsList { get; set; }
    }

    public enum TypeEnum
    { Champion, Passive, Spell };
}