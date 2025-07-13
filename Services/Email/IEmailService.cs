namespace TuiGmail.Services.Email;

public interface IEmailService
{
    Task<bool> AuthenticateAsync();
    Task<UserProfile> GetUserProfileAsync();
    UserProfile GetUserProfile();
    Task<IList<Mailbox>> GetMailboxesAsync();
    IList<Mailbox> GetMailboxes();
    Task<EmailListResult> GetEmailsAsync(string mailboxId, string? pageToken = null);
}
