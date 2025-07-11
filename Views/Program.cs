using Terminal.Gui;
using static ThemeManager;

public class Program
{
    public static void Main(string[] args)
    {
        Application.Init();
        ThemeManager.DefaultScheme = Colors.Base;
        var top = Application.Top;

        var mainWindow = new MainWindow();
        top.Add(mainWindow);

        Application.Run();
        Application.Shutdown();
    }
}