namespace LOLInfo.Tests.ViewModels
{
    using LOLInfo.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LeveltipRowViewModelTests
    {
        [TestMethod]
        public void Constructor_StoresLabelAndEffect()
        {
            var vm = new LeveltipRowViewModel("Dégâts", "60/95/130/165/200");
            Assert.AreEqual("Dégâts", vm.Label);
            Assert.AreEqual("60/95/130/165/200", vm.Effect);
        }

        [TestMethod]
        public void Effect_DoesNotContainTemplate_IsDisplayable()
        {
            var vm = new LeveltipRowViewModel("Recharge", "9/8/7/6/5");
            Assert.IsFalse(vm.Effect.Contains("{{"));
        }

        [TestMethod]
        public void Effect_ContainsTemplate_ShouldBeFilteredByCaller()
        {
            // Cette classe stocke la valeur telle quelle —
            // c'est SpellViewModel qui filtre les templates.
            var vm = new LeveltipRowViewModel("Label", "{{ basedamage }}");
            Assert.IsTrue(vm.Effect.Contains("{{"));
        }
    }

    [TestClass]
    public class FormulaRowViewModelTests
    {
        [TestMethod]
        public void Constructor_StoresLabelAndFormula()
        {
            var vm = new FormulaRowViewModel("BaseDamage", "60/95/130 + 45% PA");
            Assert.AreEqual("BaseDamage", vm.Label);
            Assert.AreEqual("60/95/130 + 45% PA", vm.Formula);
        }

        [TestMethod]
        public void Constructor_NullLabel_StoresEmptyString()
        {
            var vm = new FormulaRowViewModel(null!, "formula");
            Assert.AreEqual(string.Empty, vm.Label);
        }

        [TestMethod]
        public void Constructor_NullFormula_StoresEmptyString()
        {
            var vm = new FormulaRowViewModel("label", null!);
            Assert.AreEqual(string.Empty, vm.Formula);
        }
    }
}
