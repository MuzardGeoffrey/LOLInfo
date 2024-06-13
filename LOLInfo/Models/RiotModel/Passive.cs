namespace LOLInfo.Models.RiotModel;

using System.Text.Json.Serialization;

public class Passive
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("image")]
    public Image? Image { get; set; }
}