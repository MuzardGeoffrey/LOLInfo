namespace LOLInfo.ViewModels;

public class FormulaRowViewModel(string label, string formula)
{
    public string Label   { get; } = label   ?? string.Empty;
    public string Formula { get; } = formula ?? string.Empty;
}
