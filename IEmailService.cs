
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tui_gmail
{
    public interface IEmailService
    {
        Task<bool> AuthenticateAsync();
        Task<UserProfile> GetUserProfileAsync();
        Task<IList<Mailbox>> GetMailboxesAsync();
        Task<IList<Email>> GetEmailsAsync(string mailboxId);
    }
}
