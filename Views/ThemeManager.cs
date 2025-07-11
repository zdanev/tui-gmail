using Terminal.Gui;

public static class ThemeManager
{
    public static ColorScheme DefaultScheme { get; set; } = null!;
    public static ColorScheme DarkScheme = new ColorScheme
    {
        Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
        Focus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
        HotNormal = new Terminal.Gui.Attribute(Color.Cyan, Color.Black),
        HotFocus = new Terminal.Gui.Attribute(Color.Cyan, Color.Gray),
        Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
    };

    public static ColorScheme LightScheme = new ColorScheme
    {
        Normal = new Terminal.Gui.Attribute(Color.Black, Color.White),
        Focus = new Terminal.Gui.Attribute(Color.White, Color.DarkGray),
        HotNormal = new Terminal.Gui.Attribute(Color.Blue, Color.White),
        HotFocus = new Terminal.Gui.Attribute(Color.Blue, Color.DarkGray),
        Disabled = new Terminal.Gui.Attribute(Color.Gray, Color.White)
    };

    public static ColorScheme DarkOrangeScheme = new ColorScheme
    {
        Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
        Focus = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray),
        HotNormal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.DarkGray),
        HotFocus = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Gray),
        Disabled = new Terminal.Gui.Attribute(Color.Gray, Color.DarkGray)
    };

    private static MenuItem? _selectedTheme = null;

    public static void ApplyTheme(ColorScheme scheme, MenuItem selectedMenuItem)
    {
        Colors.Base = scheme;
        UpdateViewScheme(Application.Top, scheme);
        UpdateCheck(selectedMenuItem);
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

    private static void UpdateCheck(MenuItem newSelected)
    {
        if (_selectedTheme != null)
        {
            _selectedTheme.Checked = false;
        }
        _selectedTheme = newSelected;
        _selectedTheme.Checked = true;
    }
}