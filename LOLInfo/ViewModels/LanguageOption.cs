namespace LOLInfo.ViewModels;

/// <summary>
/// Option du sélecteur de langue : code culture .NET (ex : "fr") et nom de la langue
/// dans sa propre langue (endonyme, ex : "Français"). Le drapeau est dessiné par le
/// template XAML en fonction de <see cref="Code"/>.
/// </summary>
public sealed class LanguageOption(string code, string nativeName)
{
    public string Code       { get; } = code;
    public string NativeName { get; } = nativeName;
}
