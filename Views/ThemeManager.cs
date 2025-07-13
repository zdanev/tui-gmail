namespace TuiGmail.Views;

using Terminal.Gui;
using TuiGmail.Services.Infra;

    public static class ThemeManager
{
    public static event Action<string>? ThemeChanged;

    private static readonly SettingsService settingsService = new SettingsService();

    public static readonly Dictionary<string, ColorScheme> Themes = new()
    {
        { "Default", Colors.Base },
        { "Dark", Colors.TopLevel },
        { "Light", Colors.Dialog },
        { "Gray", Colors.Menu },
        // { "Red", Colors.Error },
    };

    public static void LoadTheme(SettingsService settingsService)
    {
        var settings = settingsService.LoadSettings();
        var themeName = settings.Theme ?? "Default";
        ApplyTheme(themeName);
    }

    public static void ApplyTheme(string themeName)
    {
        if (Themes.TryGetValue(themeName, out var scheme))
        {
            Colors.Base = scheme;
            UpdateViewScheme(Application.Top, scheme);
            ThemeChanged?.Invoke(themeName);

            var settings = settingsService.LoadSettings();
            settings.Theme = themeName;
            settingsService.SaveSettings(settings);
        }
    }

    private static void UpdateViewScheme(View view, ColorScheme scheme)
    {
        if (view != null)
        {
            view.ColorScheme = scheme;
            view.SetNeedsDisplay();
            foreach (var subView in view.Subviews)
            {
                UpdateViewScheme(subView, scheme);
            }
        }
    }
}