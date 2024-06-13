namespace LOLInfo.Models.RiotModel;

using System.Text.Json.Serialization;

public class Leveltip
{
    [JsonPropertyName("label")]
    public List<string>? Label { get; set; }

    [JsonPropertyName("effect")]
    public List<string>? Effect { get; set; }
}