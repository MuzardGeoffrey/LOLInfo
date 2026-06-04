namespace LOLInfo.Views;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using LOLInfo.Utils;

/// <summary>
/// Champ de saisie avec autocomplétion : affiche, sous le texte, une liste déroulante
/// de suggestions filtrées (cf. <see cref="AutoCompleteFilter"/>). Réutilisable :
/// recherche d'objets et sélecteur d'objet à équiper.
///
/// • <see cref="Text"/> (TwoWay) : la saisie courante.
/// • <see cref="ItemsSource"/> + <see cref="TextMemberPath"/> : les candidats et la
///   propriété servant de libellé.
/// • <see cref="SelectedItem"/> (TwoWay) : l'élément choisi dans les suggestions.
/// </summary>
public partial class AutoCompleteBox : UserControl
{
    // Évite de relancer une recherche quand on remplit le texte par programmation.
    private bool _suppress;

    private static readonly Dictionary<(Type, string), PropertyInfo?> PropCache = new();

    public AutoCompleteBox()
    {
        this.InitializeComponent();
        this.PART_Placeholder.Visibility =
            string.IsNullOrEmpty(this.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(AutoCompleteBox), new PropertyMetadata(null));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)this.GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty TextMemberPathProperty = DependencyProperty.Register(
        nameof(TextMemberPath), typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(string.Empty));

    public string TextMemberPath
    {
        get => (string)this.GetValue(TextMemberPathProperty);
        set => this.SetValue(TextMemberPathProperty, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(AutoCompleteBox),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public string Text
    {
        get => (string)this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(AutoCompleteBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedItem
    {
        get => this.GetValue(SelectedItemProperty);
        set => this.SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder), typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(string.Empty));

    public string Placeholder
    {
        get => (string)this.GetValue(PlaceholderProperty);
        set => this.SetValue(PlaceholderProperty, value);
    }

    /// <summary>Nombre maximum de suggestions affichées.</summary>
    public int MaxSuggestions { get; set; } = 8;

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (AutoCompleteBox)d;
        box.PART_Placeholder.Visibility =
            string.IsNullOrEmpty(box.Text) ? Visibility.Visible : Visibility.Collapsed;
        if (!box._suppress) box.UpdateSuggestions();
    }

    private void UpdateSuggestions()
    {
        var matches = AutoCompleteFilter.Match(this.Enumerate(), this.Text, this.Display, this.MaxSuggestions);
        this.PART_List.ItemsSource = matches;

        // Inutile d'ouvrir si l'unique suggestion est déjà exactement la saisie.
        var meaningful = matches.Count > 0 &&
                         !(matches.Count == 1 &&
                           string.Equals(this.Display(matches[0]), this.Text, StringComparison.OrdinalIgnoreCase));

        if (meaningful && this.PART_TextBox.IsKeyboardFocusWithin) this.PART_Popup.IsOpen = true;
        else this.PART_Popup.IsOpen = false;
    }

    private IEnumerable<object> Enumerate()
    {
        if (this.ItemsSource is null) yield break;
        foreach (var o in this.ItemsSource) yield return o;
    }

    private string Display(object? item)
    {
        if (item is null) return string.Empty;
        var path = this.TextMemberPath;
        if (string.IsNullOrEmpty(path)) return item.ToString() ?? string.Empty;

        var key = (item.GetType(), path);
        if (!PropCache.TryGetValue(key, out var pi))
        {
            pi = item.GetType().GetProperty(path);
            PropCache[key] = pi;
        }
        return pi?.GetValue(item)?.ToString() ?? string.Empty;
    }

    private void Commit(object? chosen)
    {
        if (chosen is null) return;

        this._suppress = true;
        this.SelectedItem = chosen;
        this.SetCurrentValue(TextProperty, this.Display(chosen));
        this._suppress = false;

        this.PART_Popup.IsOpen = false;
        this.PART_TextBox.CaretIndex = this.PART_TextBox.Text.Length;
        this.PART_TextBox.Focus();
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                if (!this.PART_Popup.IsOpen) this.UpdateSuggestions();
                if (this.PART_Popup.IsOpen && this.PART_List.Items.Count > 0)
                {
                    this.PART_List.SelectedIndex = 0;
                    (this.PART_List.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem)?.Focus();
                    e.Handled = true;
                }
                break;

            case Key.Enter:
                if (this.PART_Popup.IsOpen && this.PART_List.Items.Count > 0)
                {
                    this.Commit(this.PART_List.SelectedItem ?? this.PART_List.Items[0]);
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                if (this.PART_Popup.IsOpen) { this.PART_Popup.IsOpen = false; e.Handled = true; }
                break;
        }
    }

    private void List_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && this.PART_List.SelectedItem is not null)
        {
            this.Commit(this.PART_List.SelectedItem);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            this.PART_Popup.IsOpen = false;
            this.PART_TextBox.Focus();
            e.Handled = true;
        }
    }

    private void List_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject) is { } container)
            this.Commit(container.DataContext);
    }

    private void TextBox_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        // Ne pas fermer si le focus part vers la liste de suggestions (touche Bas).
        if (e.NewFocus is DependencyObject d && FindAncestor<ListBox>(d) == this.PART_List) return;
        this.PART_Popup.IsOpen = false;
    }

    private static T? FindAncestor<T>(DependencyObject? d) where T : DependencyObject
    {
        while (d is not null and not T) d = VisualTreeHelper.GetParent(d);
        return d as T;
    }
}
