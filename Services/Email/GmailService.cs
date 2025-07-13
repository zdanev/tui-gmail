namespace TuiGmail.Services.Email;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

public class GmailService : IEmailService
{
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/gmail-dotnet-quickstart.json
    static string[] Scopes = { Google.Apis.Gmail.v1.GmailService.Scope.GmailReadonly, Google.Apis.PeopleService.v1.PeopleServiceService.Scope.UserinfoProfile, Google.Apis.PeopleService.v1.PeopleServiceService.Scope.UserinfoEmail };
    static string ApplicationName = "TUI-Gmail";
    private UserCredential? credentials;
    private UserProfile? cachedUserProfile;
    private IList<Mailbox>? cachedMailboxes;

    public async Task<bool> AuthenticateAsync()
    {
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
            return false;
        }

        using (var stream =
            new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            try
            {
                credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
                Console.WriteLine("Credential file saved to: " + credPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
                return false;
            }
        }
    }

    public async Task<UserProfile> GetUserProfileAsync()
    {
        if (credentials == null)
        {
            throw new InvalidOperationException("Authentication required. Call AuthenticateAsync first.");
        }

        if (cachedUserProfile != null)
        {
            return cachedUserProfile;
        }

        var peopleService = new Google.Apis.PeopleService.v1.PeopleServiceService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = ApplicationName,
        });

        var profileRequest = peopleService.People.Get("people/me");
        profileRequest.PersonFields = "names,emailAddresses,locales,photos";

        var person = await profileRequest.ExecuteAsync();

        var userProfile = new UserProfile();

        if (person != null)
        {
            userProfile.FullName = person.Names?.FirstOrDefault()?.DisplayName ?? string.Empty;
            userProfile.EmailAddress = person.EmailAddresses?.FirstOrDefault()?.Value ?? string.Empty;
            userProfile.ProfilePictureUrl = person.Photos?.FirstOrDefault()?.Url ?? string.Empty;

            // Locales can contain language and country information
            var locale = person.Locales?.FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(locale))
            {
                string[] parts = locale.Split('-');
                if (parts.Length == 2)
                {
                    userProfile.Language = parts[0];
                    userProfile.Country = parts[1];
                }
                else
                {
                    userProfile.Language = locale;
                }
            }
        }
        cachedUserProfile = userProfile;
        return userProfile;
    }

    public UserProfile GetUserProfile()
    {
        return cachedUserProfile!;
    }

    public async Task<IList<Mailbox>> GetMailboxesAsync()
    {
        if (credentials == null)
        {
            throw new InvalidOperationException("Authentication required. Call AuthenticateAsync first.");
        }

        if (cachedMailboxes != null)
        {
            return cachedMailboxes;
        }

        // Create Gmail API service.
        var service = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = ApplicationName,
        });

        // Define parameters of request.
        UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

        // List labels.
        var labelsResponse = await request.ExecuteAsync();
        var labels = labelsResponse.Labels;
        if (labels == null)
        {
            return [];
        }

        var mailboxes = new List<Mailbox>();
        foreach (var label in labels)
        {
            var labelDetailsRequest = service.Users.Labels.Get("me", label.Id);
            var labelDetails = await labelDetailsRequest.ExecuteAsync();
            mailboxes.Add(new Mailbox
            {
                Id = label.Id ?? string.Empty,
                Name = label.Name ?? string.Empty,
                UnreadMessages = labelDetails.MessagesUnread ?? 0,
                TotalMessages = labelDetails.MessagesTotal ?? 0
            });
        }

        var preferredOrder = new List<string> { "INBOX", "UNREAD", "IMPORTANT", "STARRED", "DRAFT", "SENT", "SPAM", "TRASH" };
        mailboxes.Sort((a, b) => {
            var aIndex = preferredOrder.IndexOf(a.Name.ToUpper());
            var bIndex = preferredOrder.IndexOf(b.Name.ToUpper());
            if (aIndex == -1) aIndex = int.MaxValue;
            if (bIndex == -1) bIndex = int.MaxValue;
            if (aIndex == bIndex) return string.Compare(a.Name, b.Name);
            return aIndex.CompareTo(bIndex);
        });

        cachedMailboxes = mailboxes;
        return mailboxes;
    }

    public IList<Mailbox> GetMailboxes()
    {
        return cachedMailboxes!;
    }

    public async Task<EmailListResult> GetEmailsAsync(string mailboxId, string? pageToken = null)
    {
        if (credentials == null)
        {
            throw new InvalidOperationException("Authentication required. Call AuthenticateAsync first.");
        }

        var service = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = ApplicationName,
        });

        var request = service.Users.Messages.List("me");
        request.LabelIds = mailboxId;
        request.MaxResults = 20;
        request.PageToken = pageToken;

        var response = await request.ExecuteAsync();
        var emails = new List<Email>();

        if (response.Messages != null)
        {
            foreach (var message in response.Messages)
            {
                var msgRequest = service.Users.Messages.Get("me", message.Id);
                var messageDetails = await msgRequest.ExecuteAsync();
                var fromHeader = messageDetails.Payload.Headers.FirstOrDefault(h => h.Name == "From");
                var subjectHeader = messageDetails.Payload.Headers.FirstOrDefault(h => h.Name == "Subject");

                emails.Add(new Email
                {
                    From = fromHeader?.Value ?? string.Empty,
                    Subject = subjectHeader?.Value ?? string.Empty,
                    Snippet = messageDetails.Snippet ?? string.Empty,
                    ReceivedDateTime = DateTimeOffset.FromUnixTimeMilliseconds(messageDetails.InternalDate ?? 0).LocalDateTime,
                    IsUnread = messageDetails.LabelIds.Contains("UNREAD")
                });
            }
        }

        return new EmailListResult
        {
            Emails = emails,
            NextPageToken = response.NextPageToken
        };
    }
}