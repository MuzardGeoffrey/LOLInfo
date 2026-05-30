namespace LOLInfo.Models.RiotModel;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Champion
{
    [JsonPropertyName("version")]   public string? Version  { get; set; }
    [JsonPropertyName("id")]        public string? Id       { get; set; }
    [JsonPropertyName("key")]       public string? Key      { get; set; }
    [JsonPropertyName("name")]      public string? Name     { get; set; }
    [JsonPropertyName("title")]     public string? Title    { get; set; }
    [JsonPropertyName("blurb")]     public string? Blurb    { get; set; }
    [JsonPropertyName("info")]      public Info?   Info     { get; set; }
    [JsonPropertyName("image")]     public Image?  Image    { get; set; }
    [JsonPropertyName("tags")]      public List<string>? Tags    { get; set; }
    [JsonPropertyName("partype")]   public string? Partype  { get; set; }
    [JsonPropertyName("stats")]     public Dictionary<string, double>? Stats { get; set; }
    [JsonPropertyName("skins")]     public List<Skin>? Skins { get; set; }
    [JsonPropertyName("lore")]      public string? Lore     { get; set; }
    [JsonPropertyName("allytips")]  public List<string>? Allytips  { get; set; }
    [JsonPropertyName("enemytips")] public List<string>? Enemytips { get; set; }
    [JsonPropertyName("spells")]    public List<Spell>?  Spells    { get; set; }
    [JsonPropertyName("passive")]   public Passive? Passive { get; set; }
    [JsonPropertyName("recommended")] public List<object>? Recommended { get; set; }
}
