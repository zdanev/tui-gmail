namespace TuiGmail;

using Terminal.Gui;
using TuiGmail.EmailService;
using TuiGmail.Views;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("TUI Gmail Client, Copyright (c) 2025, zdanev@");
        IEmailService emailService = new GmailService();

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
        ThemeManager.DefaultScheme = Colors.Base;
        var top = Application.Top;

        var mainWindow = new MainWindow();
        top.Add(mainWindow);

        Application.Run();
        Application.Shutdown();

        Console.WriteLine("Goodbye!");
    }
}

//         try
//         {
//             var userProfile = await emailService.GetUserProfileAsync();
//             Console.WriteLine("\n--- User Profile ---");
//             Console.WriteLine($"Full Name: {userProfile.FullName}");
//             Console.WriteLine($"Email: {userProfile.EmailAddress}");
//             Console.WriteLine($"Country: {userProfile.Country}");
//             Console.WriteLine($"Language: {userProfile.Language}");
//             Console.WriteLine($"Profile Picture URL: {userProfile.ProfilePictureUrl}");
//             Console.WriteLine("--------------------\n");

//             var mailboxes = await emailService.GetMailboxesAsync();

//             if (mailboxes != null && mailboxes.Count > 0)
//             {
//                 Console.WriteLine("Mailboxes:");
//                 for (int i = 0; i < mailboxes.Count; i++)
//                 {
//                     Console.WriteLine($"[{i + 1}] {mailboxes[i].Name} ({mailboxes[i].UnreadMessages}/{mailboxes[i].TotalMessages})");
//                 }

//                 Console.Write("\nEnter the number of the mailbox to view emails: ");
//                 if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= mailboxes.Count)
//                 {
//                     var selectedMailbox = mailboxes[choice - 1];
//                     var emails = await emailService.GetEmailsAsync(selectedMailbox.Id);

//                     if (emails != null && emails.Any())
//                     {
//                         Console.WriteLine($"\nEmails in {selectedMailbox.Name}:");
//                         foreach (var email in emails)
//                         {
//                             Console.WriteLine($"From: {email.From}");
//                             Console.WriteLine($"Subject: {email.Subject}");
//                             Console.WriteLine($"Snippet: {email.Snippet}");
//                             Console.WriteLine("--------------------------------");
//                         }
//                     }
//                     else
//                     {
//                         Console.WriteLine("No emails found in this mailbox.");
//                     }
//                 }
//                 else
//                 {
//                     Console.WriteLine("Invalid choice.");
//                 }
//             }
//             else
//             {
//                 Console.WriteLine("No mailboxes found.");
//             }
//         }
//         catch (InvalidOperationException ex)
//         {
//             Console.WriteLine($"Error: {ex.Message}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"An unexpected error occurred: {ex.Message}");
//         }
//     }
// }