using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;
using TB.ViewModels;
using Windows.System;
using Windows.UI.Core;

namespace TB.Controls;

public sealed partial class NavigationBar : UserControl
{
    public NavigationBar()
    {
        this.InitializeComponent();
    }

    public void FocusUrlBar()
    {
        UrlInput?.Focus(FocusState.Programmatic);
        UrlInput?.SelectAll();
    }

    public void UpdateSecurityIcon(string url)
    {
        if (SecurityIcon == null) return;

        var resources = Application.Current.Resources;

        Geometry? GetGeometry(string key)
        {
            if (resources.TryGetValue(key, out var val) && val is string pathData)
                return XamlBindingHelper.ConvertValue(typeof(Geometry), pathData) as Geometry;
            return null;
        }

        Brush? GetBrush(string key)
        {
            return resources.TryGetValue(key, out var val) ? val as Brush : null;
        }

        SecurityIcon.Data = GetGeometry("IconLockData");
        SecurityIcon.Stroke = GetBrush("textMutedBrush");

        if (string.IsNullOrEmpty(url) || url.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
        {
            SecurityIcon.Data = null;
        }
        else if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            SecurityIcon.Data = GetGeometry("IconInfoData");
            SecurityIcon.Stroke = GetBrush("dangerBrush");
        }
        else if (url.StartsWith("tb://", StringComparison.OrdinalIgnoreCase))
        {
            SecurityIcon.Data = GetGeometry("IconSettingsGearData");
            SecurityIcon.Stroke = GetBrush("accentBrush");
        }
    }

    private void OnUrlInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            bool ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);

            if (ctrl && UrlInput is not null)
            {
                var text = UrlInput.Text.Trim();
                if (!string.IsNullOrWhiteSpace(text) && !text.Contains('.') && !text.Contains('/'))
                    UrlInput.Text = $"www.{text}.com";
            }

            if (DataContext is ChromeViewModel vm)
            {
                vm.UrlText = UrlInput?.Text ?? "";
                vm.NavigateCommand.Execute(null);
            }
            e.Handled = true;
        }
    }

    private void OnUrlInputGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.Parent is Grid grid && grid.Parent is Border border)
        {
            var themeKey = Application.Current.RequestedTheme == ApplicationTheme.Light ? "Light" : "Dark";
            var dict = Application.Current.Resources.ThemeDictionaries[themeKey] as ResourceDictionary;
            if (dict?.TryGetValue("accentBrush", out var brush) == true)
                border.BorderBrush = (Brush)brush;

            if (DataContext is ChromeViewModel vm)
            {
                tb.Text = vm.UrlText;
                tb.SelectAll();
            }
        }
    }

    private void OnUrlInputLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            if (tb.Parent is Grid grid && grid.Parent is Border border)
            {
                var themeKey = Application.Current.RequestedTheme == ApplicationTheme.Light ? "Light" : "Dark";
                var dict = Application.Current.Resources.ThemeDictionaries[themeKey] as ResourceDictionary;
                if (dict?.TryGetValue("borderCrispBrush", out var brush) == true)
                    border.BorderBrush = (Brush)brush;
            }
        }
    }
}

