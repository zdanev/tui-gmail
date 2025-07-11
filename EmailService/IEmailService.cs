namespace TuiGmail.EmailService;

public interface IEmailService
{
    Task<bool> AuthenticateAsync();
    Task<UserProfile> GetUserProfileAsync();
    Task<IList<Mailbox>> GetMailboxesAsync();
    Task<IList<Email>> GetEmailsAsync(string mailboxId);
}
