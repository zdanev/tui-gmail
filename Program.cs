using System;
using System.Linq;
using System.Threading.Tasks;

namespace tui_gmail
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IEmailService emailService = new GmailService();
            var mailboxes = await emailService.GetMailboxesAsync();

            if (mailboxes != null && mailboxes.Count > 0)
            {
                Console.WriteLine("Mailboxes:");
                for (int i = 0; i < mailboxes.Count; i++)
                {
                    Console.WriteLine($"[{i + 1}] {mailboxes[i].Name} ({mailboxes[i].UnreadMessages})");
                }

                Console.Write("\nEnter the number of the mailbox to view emails: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= mailboxes.Count)
                {
                    var selectedMailbox = mailboxes[choice - 1];
                    var emails = await emailService.GetEmailsAsync(selectedMailbox.Id);

                    if (emails != null && emails.Any())
                    {
                        Console.WriteLine($"\nEmails in {selectedMailbox.Name}:");
                        foreach (var email in emails)
                        {
                            Console.WriteLine($"From: {email.From}");
                            Console.WriteLine($"Subject: {email.Subject}");
                            Console.WriteLine($"Snippet: {email.Snippet}");
                            Console.WriteLine("--------------------------------");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No emails found in this mailbox.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid choice.");
                }
            }
            else
            {
                Console.WriteLine("No mailboxes found.");
            }
        }
    }
}
