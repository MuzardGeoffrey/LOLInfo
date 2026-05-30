namespace LOLInfo.Tests.ViewModels
{
    using LOLInfo.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FilterItemViewModelTests
    {
        [TestMethod]
        public void Label_IsStoredCorrectly()
        {
            var vm = new FilterItemViewModel("Mage");
            Assert.AreEqual("Mage", vm.Label);
        }

        [TestMethod]
        public void IsSelected_DefaultFalse()
        {
            var vm = new FilterItemViewModel("Fighter");
            Assert.IsFalse(vm.IsSelected);
        }

        [TestMethod]
        public void IsSelected_InitialTrue_WhenPassedInConstructor()
        {
            var vm = new FilterItemViewModel("Tank", isSelected: true);
            Assert.IsTrue(vm.IsSelected);
        }

        [TestMethod]
        public void IsSelected_Set_RaisesPropertyChanged()
        {
            var vm = new FilterItemViewModel("Mage");
            bool raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(FilterItemViewModel.IsSelected))
                    raised = true;
            };

            vm.IsSelected = true;
            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void IsSelected_SetSameValue_DoesNotRaisePropertyChanged()
        {
            var vm = new FilterItemViewModel("Mage", isSelected: false);
            int count = 0;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(FilterItemViewModel.IsSelected))
                    count++;
            };

            vm.IsSelected = false; // même valeur → pas de notification
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void IsSelected_Toggle_RaisesPropertyChangedTwice()
        {
            var vm = new FilterItemViewModel("Mage");
            int count = 0;
            vm.PropertyChanged += (_, _) => count++;

            vm.IsSelected = true;
            vm.IsSelected = false;
            Assert.AreEqual(2, count);
        }
    }
}
