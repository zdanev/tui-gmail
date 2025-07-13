namespace TuiGmail.Services.Email;

public class Email
{
    public string From { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public DateTime ReceivedDateTime { get; set; }
    public bool IsUnread { get; set; }
}
