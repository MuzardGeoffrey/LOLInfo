namespace LOLInfo.Tests.CdragonModel.Parts
{
    using System.Collections.Generic;
    using System.Linq;
    using LOLInfo.Models.CdragonModel;
    using LOLInfo.Models.CdragonModel.Parts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NamedDataValuePartTests
    {
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<double>> SampleData =
            new Dictionary<string, IReadOnlyList<double>>
            {
                ["BaseDamage"] = new List<double> { 60, 95, 130, 165, 200 },
                ["APRatio"]    = new List<double> { 0.45 },
            };

        [TestMethod]
        public void Format_KnownKey_ReturnsSlashSeparatedValues()
        {
            var part = new NamedDataValuePart("BaseDamage", SampleData);
            Assert.AreEqual("60/95/130/165/200", part.Format());
        }

        [TestMethod]
        public void Format_SingleValue_NoSlash()
        {
            var part = new NamedDataValuePart("APRatio", SampleData);
            Assert.AreEqual("0.45", part.Format());
        }

        [TestMethod]
        public void Format_UnknownKey_ReturnsQuestionMark()
        {
            var part = new NamedDataValuePart("DoesNotExist", SampleData);
            Assert.AreEqual("?", part.Format());
        }

        [TestMethod]
        public void Format_NullDictionary_ReturnsQuestionMark()
        {
            var part = new NamedDataValuePart("BaseDamage", null);
            Assert.AreEqual("?", part.Format());
        }

        [TestMethod]
        public void DataValueName_IsStoredCorrectly()
        {
            var part = new NamedDataValuePart("BaseDamage", SampleData);
            Assert.AreEqual("BaseDamage", part.DataValueName);
        }

        [TestMethod]
        public void Constructor_NullName_StoresEmptyString()
        {
            var part = new NamedDataValuePart(null!, SampleData);
            Assert.AreEqual(string.Empty, part.DataValueName);
        }
    }
}
