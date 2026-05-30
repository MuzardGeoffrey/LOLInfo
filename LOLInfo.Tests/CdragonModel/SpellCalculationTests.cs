namespace LOLInfo.Tests.CdragonModel
{
    using System.Collections.Generic;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SpellCalculationTests
    {
        [TestMethod]
        public void Format_SinglePart_ReturnsPartFormat()
        {
            var calc = new SpellCalculation("Damage", new List<IFormulaPart> { new NumberPart(100) });
            Assert.AreEqual("100", calc.Format());
        }

        [TestMethod]
        public void Format_MultipleParts_JoinedWithPlus()
        {
            var parts = new List<IFormulaPart>
            {
                new NumberPart(60),
                new NamedDataValuePart("APRatio",
                    new Dictionary<string, IReadOnlyList<double>> { ["APRatio"] = new[] { 0.45 } }),
            };
            var calc = new SpellCalculation("Damage", parts);
            Assert.AreEqual("60 + 0.45", calc.Format());
        }

        [TestMethod]
        public void Format_EmptyParts_ReturnsEmptyString()
        {
            var calc = new SpellCalculation("Damage", new List<IFormulaPart>());
            Assert.AreEqual(string.Empty, calc.Format());
        }

        [TestMethod]
        public void Format_NullParts_ReturnsEmptyString()
        {
            var calc = new SpellCalculation("Damage", null!);
            Assert.AreEqual(string.Empty, calc.Format());
        }

        [TestMethod]
        public void Name_IsStoredCorrectly()
        {
            var calc = new SpellCalculation("BaseDamage", new List<IFormulaPart>());
            Assert.AreEqual("BaseDamage", calc.Name);
        }

        [TestMethod]
        public void Constructor_NullName_StoresEmptyString()
        {
            var calc = new SpellCalculation(null!, new List<IFormulaPart>());
            Assert.AreEqual(string.Empty, calc.Name);
        }
    }
}
