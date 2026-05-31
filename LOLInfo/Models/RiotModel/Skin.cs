namespace LOLInfo.Models.RiotModel;

using System.Text.Json.Serialization;

public class Skin
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("num")]
    public int? Num { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("chromas")]
    public bool? Chromas { get; set; }

    /// <summary>
    /// Num du skin parent si cette entrée est une <b>variante de couleur (chroma)</b>.
    /// Null pour un vrai skin. DataDragon liste les chromas comme des skins à part
    /// entière ; ce champ permet de les distinguer.
    /// </summary>
    [JsonPropertyName("parentSkin")]
    public int? ParentSkin { get; set; }
}