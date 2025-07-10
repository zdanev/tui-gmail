using System;
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
                foreach (var mailbox in mailboxes)
                {
                    Console.WriteLine("{0}", mailbox.Name);
                }
            }
            else
            {
                Console.WriteLine("No mailboxes found.");
            }
        }
    }
}
