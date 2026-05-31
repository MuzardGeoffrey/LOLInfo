namespace LOLInfo.Tests.Models;

using System.Linq;
using System.Text.Json;

using LOLInfo.Models.RiotModel;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Régression : de nombreux champions (ex : Akali Q = 1.5, Akali E = 14.5) ont des
/// valeurs DÉCIMALES dans cooldown/cost/range/effect. Si le modèle les type en int,
/// System.Text.Json lève une exception et toute la désérialisation du champion échoue
/// (sorts vides → ArgumentOutOfRangeException sur Spells[0..4] dans la vue).
/// </summary>
[TestClass]
public class SpellDeserializationTests
{
    // Reproduit la structure réelle du JSON de détail (enveloppe data + valeurs décimales).
    private const string AkaliLikeJson = """
    {
      "type": "champion",
      "data": {
        "Akali": {
          "id": "Akali",
          "name": "Akali",
          "passive": { "name": "Assassin's Mark", "description": "...", "image": { "full": "Akali_P.png" } },
          "spells": [
            { "id": "AkaliQ", "name": "Five Point Strike", "description": "...",
              "cooldown": [1.5, 1.5, 1.5, 1.5, 1.5], "cooldownBurn": "1.5",
              "cost": [120, 115, 110, 105, 100], "costBurn": "120/115/110/105/100",
              "range": [550, 550, 550, 550, 550], "rangeBurn": "550",
              "effect": [null, [0.3, 0.35, 0.4, 0.45, 0.5]],
              "image": { "full": "AkaliQ.png" } },
            { "id": "AkaliE", "name": "Shuriken Flip", "description": "...",
              "cooldown": [16, 14.5, 13, 11.5, 10], "cooldownBurn": "16/14.5/13/11.5/10",
              "image": { "full": "AkaliE.png" } }
          ]
        }
      }
    }
    """;

    [TestMethod]
    public void Deserialize_SpellsWithDecimalValues_DoesNotThrow_AndKeepsSpells()
    {
        var result = JsonSerializer.Deserialize<JsonRiotFormat>(AkaliLikeJson);

        var champion = result?.ChampionsList?.Values.FirstOrDefault();
        Assert.IsNotNull(champion);
        Assert.IsNotNull(champion!.Spells);
        Assert.AreEqual(2, champion.Spells!.Count);

        // Les valeurs décimales sont bien préservées (double, pas int).
        Assert.AreEqual(1.5, champion.Spells[0].Cooldown![0]);
        Assert.AreEqual(14.5, champion.Spells[1].Cooldown![1]);
        Assert.AreEqual(0.3, champion.Spells[0].Effect![1]![0]);
    }
}
