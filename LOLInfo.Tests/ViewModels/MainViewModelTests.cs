namespace LOLInfo.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using LOLInfo.IServices;
    using LOLInfo.IServices.Storage;
    using LOLInfo.IViewModels;
    using LOLInfo.ViewModels;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MainViewModelTests
    {
        private static MainViewModel CreateVm(string current = "fr", params string[] available)
        {
            var view = new Mock<IViewManager>();
            var items = new Mock<IItemsViewModel>();

            var lang = new Mock<ILanguageService>();
            lang.Setup(l => l.CurrentLanguage).Returns(current);
            lang.Setup(l => l.AvailableLanguages)
                .Returns(available.Length > 0 ? available : new[] { "en", "fr" });

            return new MainViewModel(view.Object, items.Object, lang.Object);
        }

        [TestMethod]
        public void Languages_BuiltFromAvailableLanguages_WithNativeNames()
        {
            var vm = CreateVm("fr", "en", "fr");

            Assert.AreEqual(2, vm.Languages.Count);

            var fr = vm.Languages.First(l => l.Code == "fr");
            var en = vm.Languages.First(l => l.Code == "en");
            Assert.AreEqual("Français", fr.NativeName);
            Assert.AreEqual("English", en.NativeName);
        }

        [TestMethod]
        public void CurrentLanguageCode_ReflectsLanguageService()
        {
            var vm = CreateVm("en", "en", "fr");
            Assert.AreEqual("en", vm.CurrentLanguageCode);
        }
    }
}
