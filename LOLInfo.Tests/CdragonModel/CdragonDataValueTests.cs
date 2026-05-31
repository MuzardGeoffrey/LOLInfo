namespace LOLInfo.Tests.CdragonModel
{
    using System.Text.Json;

    using LOLInfo.Models.CdragonModel;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Régression : les DataValues du bin.json CDragon utilisent les clés
    /// "name"/"values" (et NON "mName"/"mValues"). Un mauvais mapping faisait
    /// ressortir toutes les valeurs vides → formules affichées « ? ».
    /// </summary>
    [TestClass]
    public class CdragonDataValueTests
    {
        [TestMethod]
        public void Deserialize_UsesNameAndValuesKeys()
        {
            const string json =
                """{"name":"BaseDamage","values":[0,30,60,90,120,150,180],"__type":"SpellDataValue"}""";

            var dv = JsonSerializer.Deserialize<CdragonDataValue>(json);

            Assert.IsNotNull(dv);
            Assert.AreEqual("BaseDamage", dv!.Name);
            Assert.IsNotNull(dv.Values);
            Assert.AreEqual(7, dv.Values!.Count);
            Assert.AreEqual(30.0, dv.Values[1]);
        }

        [TestMethod]
        public void Parser_ResolvesNamedDataValue_FromRealBinJsonShape()
        {
            // Structure réelle : mSpell.DataValues[].name/values + mSpellCalculations.
            const string json =
                """
                {
                  "mSpell": {
                    "DataValues": [
                      { "name": "BaseDamage", "values": [0,30,60,90,120,150,180], "__type": "SpellDataValue" }
                    ],
                    "mSpellCalculations": {
                      "TotalDamage": {
                        "mFormulaParts": [
                          { "__type": "NamedDataValueCalculationPart", "mDataValue": "BaseDamage" }
                        ]
                      }
                    }
                  }
                }
                """;

            var wrapper = JsonSerializer.Deserialize<CdragonSpellObjectRaw>(json);
            var calcs = CdragonChampionParser.ParseCalculations(wrapper!.SpellData);

            Assert.IsTrue(calcs.ContainsKey("TotalDamage"));
            // Avant le fix : "?". Après : les vraies valeurs par rang.
            Assert.AreEqual("0/30/60/90/120/150/180", calcs["TotalDamage"].Format());
        }
    }
}
