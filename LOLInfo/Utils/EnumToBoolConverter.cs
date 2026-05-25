namespace LOLInfo.Utils
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Convertisseur générique pour lier un RadioButton à une valeur d'enum.
    ///
    /// Utilisation en XAML :
    ///   IsChecked="{Binding MaProprieteEnum, Mode=TwoWay,
    ///               Converter={StaticResource EnumToBoolConverter},
    ///               ConverterParameter={x:Static models:MonEnum.MaValeur}}"
    ///
    /// Convert  : retourne true si la valeur courante == ConverterParameter.
    /// ConvertBack : retourne le ConverterParameter (la valeur enum du bouton coché).
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Appelé par WPF pour déterminer si ce RadioButton doit être coché.
        /// value           = valeur actuelle de la propriété dans le ViewModel.
        /// parameter       = valeur enum attendue pour CE bouton (ConverterParameter).
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.Equals(parameter) ?? false;

        /// <summary>
        /// Appelé par WPF quand l'utilisateur coche CE RadioButton.
        /// Retourne simplement la valeur enum associée à ce bouton,
        /// qui sera assignée à la propriété du ViewModel.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Si le bouton est coché (value == true), on retourne la valeur enum.
            // Si le bouton est décoché, on retourne Binding.DoNothing pour ne pas
            // écraser la propriété (WPF appellera ConvertBack sur tous les boutons du groupe).
            return value is true ? parameter : Binding.DoNothing;
        }
    }
}
