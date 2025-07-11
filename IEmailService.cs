
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tui_gmail
{
    public interface IEmailService
    {
        Task<IList<Mailbox>?> GetMailboxesAsync();
        Task<IList<Email>> GetEmailsAsync(string mailboxId);
    }
}
