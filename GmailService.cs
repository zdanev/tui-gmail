
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System; 
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace tui_gmail
{
    public class GmailService : IEmailService
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/gmail-dotnet-quickstart.json
        static string[] Scopes = { Google.Apis.Gmail.v1.GmailService.Scope.GmailReadonly };
        static string ApplicationName = "TUI-Gmail";

        public async Task<IList<Mailbox>> GetMailboxesAsync()
        {
            UserCredential credential;

            if (!File.Exists("credentials.json"))
            {
                Console.WriteLine("credentials.json not found. Please follow these steps:");
                Console.WriteLine("1. Go to the Google Cloud Console: https://console.cloud.google.com/");
                Console.WriteLine("2. Create a new project or select an existing one.");
                Console.WriteLine("3. Enable the Gmail API for your project.");
                Console.WriteLine("4. Create credentials for a 'Desktop app'.");
                Console.WriteLine("5. Download the credentials file and rename it to 'credentials.json'.");
                Console.WriteLine("6. Place 'credentials.json' in the same directory as the application executable.");
                Console.WriteLine("You can use 'credentials.json.sample' as a template.");
                return null;
            }

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            var service = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

            // List labels.
            var labels = (await request.ExecuteAsync()).Labels;
            return labels.Select(l => new Mailbox { Id = l.Id, Name = l.Name }).ToList();
        }
    }
}
