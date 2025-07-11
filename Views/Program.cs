using Terminal.Gui;

public class Program
{
    public static void Main(string[] args)
    {
        Application.Init();
        var top = Application.Top;

        var mainWindow = new MainWindow();
        top.Add(mainWindow);

        Application.Run();
        Application.Shutdown();
    }
}