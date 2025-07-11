namespace TuiGmail.EmailService;

public class Mailbox
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UnreadMessages { get; set; }
    public int TotalMessages { get; set; }
}
