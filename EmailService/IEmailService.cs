namespace TuiGmail.EmailService;

public interface IEmailService
{
    Task<bool> AuthenticateAsync();
    Task<UserProfile> GetUserProfileAsync();
    UserProfile GetUserProfile();
    Task<IList<Mailbox>> GetMailboxesAsync();
    IList<Mailbox> GetMailboxes();
    Task<IList<Email>> GetEmailsAsync(string mailboxId);
}
