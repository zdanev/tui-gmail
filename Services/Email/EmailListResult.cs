namespace TuiGmail.Services.Email;

public class EmailListResult
{
    public IList<Email> Emails { get; set; } = [];
    public string? NextPageToken { get; set; }
}