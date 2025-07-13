namespace TuiGmail;

using Terminal.Gui;
using TuiGmail.Services.Email;
using TuiGmail.Services.Infra;
using TuiGmail.Views;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("TUI Gmail Client, Copyright (c) 2025, zdanev@");
        var emailService = new GmailService();
        var settingsService = new SettingsService();

        Console.WriteLine("Authenticating...");
        if (!await emailService.AuthenticateAsync())
        {
            Console.WriteLine("Authentication failed. Exiting application.");
            return;
        }

        Console.WriteLine("Getting user profile...");
        var userProfile = await emailService.GetUserProfileAsync();

        Console.WriteLine("Getting mailboxes...");
        var mailboxes = await emailService.GetMailboxesAsync();

        Console.WriteLine("Launching TUI...");

        Application.Init();
        var top = Application.Top;

        var mainWindow = new MainWindow(emailService, settingsService);
        top.Add(mainWindow);

        ThemeManager.LoadTheme(settingsService);

        Application.Run();
        Application.Shutdown();

        Console.WriteLine("Goodbye!");
    }
}
