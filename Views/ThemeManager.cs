namespace TuiGmail.Views;

using Terminal.Gui;
using TuiGmail.Services.Infra;

public static class ThemeManager
{
    public static event Action<string>? ThemeChanged;

    private static readonly SettingsService settingsService = new SettingsService();

    public static ColorScheme DefaultScheme { get; set; } = new ColorScheme();

    public static readonly Dictionary<string, ColorScheme> Themes = new()
    {
        { "Default", DefaultScheme },
        { "Dark", new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.White, Color.Black), Focus = new Terminal.Gui.Attribute(Color.Black, Color.Gray), HotNormal = new Terminal.Gui.Attribute(Color.Cyan, Color.Black), HotFocus = new Terminal.Gui.Attribute(Color.Cyan, Color.Gray), Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black) } },
        { "Light", new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.Black, Color.White), Focus = new Terminal.Gui.Attribute(Color.White, Color.DarkGray), HotNormal = new Terminal.Gui.Attribute(Color.Blue, Color.White), HotFocus = new Terminal.Gui.Attribute(Color.Blue, Color.DarkGray), Disabled = new Terminal.Gui.Attribute(Color.Gray, Color.White) } },
        { "Dark Orange", new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black), Focus = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray), HotNormal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.DarkGray), HotFocus = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Gray), Disabled = new Terminal.Gui.Attribute(Color.Gray, Color.DarkGray) } }
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