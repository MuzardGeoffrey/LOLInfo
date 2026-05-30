namespace LOLInfo.Tests.CdragonModel
{
    using System.Collections.Generic;
    using System.Linq;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests unitaires pour CdragonChampionParser.ParseCalculations.
    ///
    /// On construit des CdragonSpellRaw manuellement pour valider
    /// que chaque __type produit bien le bon IFormulaPart.
    /// </summary>
    [TestClass]
    public class CdragonChampionParserTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        private static CdragonSpellRaw SpellWith(string calcName, CdragonFormulaPartRaw part)
            => new CdragonSpellRaw
            {
                SpellCalculations = new Dictionary<string, CdragonSpellCalculationRaw>
                {
                    [calcName] = new CdragonSpellCalculationRaw
                    {
                        FormulaParts = new List<CdragonFormulaPartRaw> { part }
                    }
                }
            };

        // ── Cas null / vide ────────────────────────────────────────────────

        [TestMethod]
        public void ParseCalculations_NullRaw_ReturnsEmpty()
        {
            var result = CdragonChampionParser.ParseCalculations(null);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParseCalculations_NoSpellCalculations_ReturnsEmpty()
        {
            var raw = new CdragonSpellRaw { SpellCalculations = null };
            var result = CdragonChampionParser.ParseCalculations(raw);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParseCalculations_EmptyFormulaParts_ReturnsEmptyParts()
        {
            var raw = new CdragonSpellRaw
            {
                SpellCalculations = new Dictionary<string, CdragonSpellCalculationRaw>
                {
                    ["Dmg"] = new CdragonSpellCalculationRaw { FormulaParts = new List<CdragonFormulaPartRaw>() }
                }
            };
            var result = CdragonChampionParser.ParseCalculations(raw);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result["Dmg"].Parts.Count);
        }

        // ── NumberCalculationPart ─────────────────────────────────────────

        [TestMethod]
        public void ParseCalculations_NumberPart_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw { Type = "NumberCalculationPart", Number = 42.0 };
            var raw = SpellWith("Calc", partRaw);

            var result = CdragonChampionParser.ParseCalculations(raw);

            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(NumberPart));
            var part = (NumberPart)result["Calc"].Parts[0];
            Assert.AreEqual(42.0, part.Value);
        }

        // ── EffectValueCalculationPart ────────────────────────────────────

        [TestMethod]
        public void ParseCalculations_EffectValuePart_ParsedCorrectly()
        {
            var raw = new CdragonSpellRaw
            {
                EffectAmount = new List<List<double>>
                {
                    new() { 0 },
                    new() { 60, 95, 130 },
                },
                SpellCalculations = new Dictionary<string, CdragonSpellCalculationRaw>
                {
                    ["Calc"] = new CdragonSpellCalculationRaw
                    {
                        FormulaParts = new List<CdragonFormulaPartRaw>
                        {
                            new() { Type = "EffectValueCalculationPart", EffectIndex = 1 }
                        }
                    }
                }
            };

            var result = CdragonChampionParser.ParseCalculations(raw);
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(EffectValuePart));
            var part = (EffectValuePart)result["Calc"].Parts[0];
            CollectionAssert.AreEqual(new[] { 60.0, 95.0, 130.0 }, part.Values.ToArray());
        }

        [TestMethod]
        public void ParseCalculations_EffectValuePart_MissingEffectIndex_Filtered()
        {
            var partRaw = new CdragonFormulaPartRaw { Type = "EffectValueCalculationPart", EffectIndex = null };
            var raw = SpellWith("Calc", partRaw);

            var result = CdragonChampionParser.ParseCalculations(raw);
            Assert.AreEqual(0, result["Calc"].Parts.Count);
        }

        // ── NamedDataValueCalculationPart ─────────────────────────────────

        [TestMethod]
        public void ParseCalculations_NamedDataValuePart_ParsedCorrectly()
        {
            var raw = new CdragonSpellRaw
            {
                DataValues = new List<CdragonDataValue>
                {
                    new() { Name = "BaseDamage", Values = new List<double> { 100, 150 } }
                },
                SpellCalculations = new Dictionary<string, CdragonSpellCalculationRaw>
                {
                    ["Calc"] = new CdragonSpellCalculationRaw
                    {
                        FormulaParts = new List<CdragonFormulaPartRaw>
                        {
                            new() { Type = "NamedDataValueCalculationPart", DataValue = "BaseDamage" }
                        }
                    }
                }
            };

            var result = CdragonChampionParser.ParseCalculations(raw);
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(NamedDataValuePart));
            var part = (NamedDataValuePart)result["Calc"].Parts[0];
            Assert.AreEqual("BaseDamage", part.DataValueName);
            CollectionAssert.AreEqual(new[] { 100.0, 150.0 }, part.Values.ToArray());
        }

        // ── StatByNamedDataValueCalculationPart ───────────────────────────

        [TestMethod]
        public void ParseCalculations_StatByNamedDataValue_ParsedCorrectly()
        {
            var raw = new CdragonSpellRaw
            {
                DataValues = new List<CdragonDataValue>
                {
                    new() { Name = "APRatio", Values = new List<double> { 0.45 } }
                },
                SpellCalculations = new Dictionary<string, CdragonSpellCalculationRaw>
                {
                    ["Calc"] = new CdragonSpellCalculationRaw
                    {
                        FormulaParts = new List<CdragonFormulaPartRaw>
                        {
                            new() { Type = "StatByNamedDataValueCalculationPart", Stat = 3, StatFormula = 2, DataValue = "APRatio" }
                        }
                    }
                }
            };

            var result = CdragonChampionParser.ParseCalculations(raw);
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(StatScalingPart));
            var part = (StatScalingPart)result["Calc"].Parts[0];
            Assert.AreEqual(ChampionStat.AbilityPower, part.Stat);
        }

        // ── StatByCoefficientCalculationPart ─────────────────────────────

        [TestMethod]
        public void ParseCalculations_StatByCoefficientPart_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "StatByCoefficientCalculationPart",
                Coefficient = 0.30,
                Stat = 3,
                StatFormula = 2
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(StatByCoefficientPart));
            var part = (StatByCoefficientPart)result["Calc"].Parts[0];
            Assert.AreEqual(0.30, part.Coefficient);
        }

        // ── ByCharLevelInterpolationCalculationPart ───────────────────────

        [TestMethod]
        public void ParseCalculations_ByLevelInterpolation_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "ByCharLevelInterpolationCalculationPart",
                StartValue = 75,
                EndValue = 165
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(ByLevelInterpolationPart));
            var part = (ByLevelInterpolationPart)result["Calc"].Parts[0];
            Assert.AreEqual(75, part.StartValue);
            Assert.AreEqual(165, part.EndValue);
        }

        // ── ByCharLevelFormulaCalculationPart ─────────────────────────────

        [TestMethod]
        public void ParseCalculations_ByLevelFormula_ParsedCorrectly()
        {
            var values = new List<double> { 10, 20, 30 };
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "ByCharLevelFormulaCalculationPart",
                LevelValues = values
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(ByLevelFormulaPart));
            var part = (ByLevelFormulaPart)result["Calc"].Parts[0];
            Assert.AreEqual(3, part.Values.Count);
        }

        // ── ByCharLevelBreakpointsCalculationPart ─────────────────────────

        [TestMethod]
        public void ParseCalculations_ByLevelBreakpoints_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "ByCharLevelBreakpointsCalculationPart",
                Level1Value = 55,
                Breakpoints = new List<CdragonBreakpoint>
                {
                    new() { Level = 7, AdditionalBonus = -5 }
                }
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(ByLevelBreakpointsPart));
            var part = (ByLevelBreakpointsPart)result["Calc"].Parts[0];
            Assert.AreEqual(55, part.Level1Value);
            Assert.AreEqual(1, part.Breakpoints.Count);
        }

        // ── BuffCounterByCoefficientCalculationPart ───────────────────────

        [TestMethod]
        public void ParseCalculations_BuffCounterCoefficient_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "BuffCounterByCoefficientCalculationPart",
                Coefficient = 1.7
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(BuffCounterCoefficientPart));
            var part = (BuffCounterCoefficientPart)result["Calc"].Parts[0];
            Assert.AreEqual(1.7, part.Coefficient);
        }

        // ── AbilityResourceByCoefficientCalculationPart ───────────────────

        [TestMethod]
        public void ParseCalculations_AbilityResourceCoefficient_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "AbilityResourceByCoefficientCalculationPart",
                Coefficient = 0.02
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(AbilityResourceCoefficientPart));
            var part = (AbilityResourceCoefficientPart)result["Calc"].Parts[0];
            Assert.AreEqual(0.02, part.Coefficient);
        }

        // ── ProductOfSubPartsCalculationPart ──────────────────────────────

        [TestMethod]
        public void ParseCalculations_ProductOfSubParts_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "ProductOfSubPartsCalculationPart",
                SubParts = new List<CdragonFormulaPartRaw>
                {
                    new() { Type = "NumberCalculationPart", Number = 2 },
                    new() { Type = "NumberCalculationPart", Number = 3 },
                }
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(ProductOfSubPartsPart));
            var part = (ProductOfSubPartsPart)result["Calc"].Parts[0];
            Assert.AreEqual(2, part.SubParts.Count);
        }

        // ── SumOfSubPartsCalculationPart ──────────────────────────────────

        [TestMethod]
        public void ParseCalculations_SumOfSubParts_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "SumOfSubPartsCalculationPart",
                SubParts = new List<CdragonFormulaPartRaw>
                {
                    new() { Type = "NumberCalculationPart", Number = 10 },
                    new() { Type = "NumberCalculationPart", Number = 20 },
                }
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(SumOfSubPartsPart));
            var part = (SumOfSubPartsPart)result["Calc"].Parts[0];
            Assert.AreEqual(2, part.SubParts.Count);
        }

        // ── ClampSubPartsCalculationPart ──────────────────────────────────

        [TestMethod]
        public void ParseCalculations_ClampSubParts_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "ClampSubPartsCalculationPart",
                SubParts = new List<CdragonFormulaPartRaw>
                {
                    new() { Type = "NumberCalculationPart", Number = 50 },
                    new() { Type = "NumberCalculationPart", Number = 0  },
                    new() { Type = "NumberCalculationPart", Number = 100 },
                }
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(ClampSubPartsPart));
            var part = (ClampSubPartsPart)result["Calc"].Parts[0];
            Assert.AreEqual(3, part.SubParts.Count);
        }

        // ── StatBySubPartCalculationPart ──────────────────────────────────

        [TestMethod]
        public void ParseCalculations_StatBySubPart_ParsedCorrectly()
        {
            var partRaw = new CdragonFormulaPartRaw
            {
                Type = "StatBySubPartCalculationPart",
                Stat = 3,
                StatFormula = 2,
                SubPart = new CdragonFormulaPartRaw { Type = "NumberCalculationPart", Number = 0.45 }
            };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(StatBySubPartPart));
            var part = (StatBySubPartPart)result["Calc"].Parts[0];
            Assert.AreEqual(ChampionStat.AbilityPower, part.Stat);
        }

        // ── Type inconnu → UnknownPart ────────────────────────────────────

        [TestMethod]
        public void ParseCalculations_UnknownType_ReturnsUnknownPart()
        {
            var partRaw = new CdragonFormulaPartRaw { Type = "{ee18a47b}" };
            var result = CdragonChampionParser.ParseCalculations(SpellWith("Calc", partRaw));
            Assert.IsInstanceOfType(result["Calc"].Parts[0], typeof(UnknownPart));
        }

        // ── GetDataValues : fusion legacy + nouveau format ────────────────

        [TestMethod]
        public void GetDataValues_PrefersNewFormatOverLegacy()
        {
            var raw = new CdragonSpellRaw
            {
                DataValuesLegacy = new List<CdragonDataValue>
                {
                    new() { Name = "X", Values = new List<double> { 1 } }
                },
                DataValues = new List<CdragonDataValue>
                {
                    new() { Name = "Y", Values = new List<double> { 2 } }
                }
            };
            var merged = raw.GetDataValues();
            Assert.IsTrue(merged.Any(dv => dv.Name == "X"));
            Assert.IsTrue(merged.Any(dv => dv.Name == "Y"));
        }
    }
}
