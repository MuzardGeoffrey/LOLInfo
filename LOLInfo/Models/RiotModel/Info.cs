namespace LOLInfo.Models.RiotModel;

using System.Text.Json.Serialization;

public class Info
{
    [JsonPropertyName("attack")]     public int? Attack     { get; set; }
    [JsonPropertyName("defense")]    public int? Defense    { get; set; }
    [JsonPropertyName("magic")]      public int? Magic      { get; set; }
    [JsonPropertyName("difficulty")] public int? Difficulty { get; set; }
}
