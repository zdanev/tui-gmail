namespace TuiGmail.Views;

using System.Data;
using Terminal.Gui;
using Terminal.Gui.Graphs;
using System.Linq;
using TuiGmail.Services.Email;
using TuiGmail.Services.Infra;

public class MainWindow : Window
{
    private readonly IEmailService emailService;
    private readonly SettingsService settingsService;
    private readonly UserProfile userProfile;
    private readonly Settings settings;
    private readonly IList<Mailbox> mailboxes = [];

    private readonly ListView mailboxesListView;
    private readonly DataTable emailsDataTable;
    private readonly TableView messagesView;
    private readonly EmailView emailView;
    private readonly List<MenuItem> themeMenuItems = [];
    private readonly MenuItem showUnreadCountMenuItem;
    private readonly StatusBar statusBar;

    private string? nextPageToken;

    public MainWindow(IEmailService emailService, SettingsService settingsService) : base("TUI Gmail")
    {
        this.emailService = emailService;
        this.settingsService = settingsService;
        this.settings = settingsService.LoadSettings();
        this.mailboxes = emailService.GetMailboxes() ?? [];
        this.userProfile = emailService.GetUserProfile();

        Title = userProfile.EmailAddress;
        X = 0;
        Y = 1; // Leave one row for the top-level menu

        var themeItems = new List<MenuItem>();
        foreach (var themeName in ThemeManager.Themes.Keys)
        {
            var themeMenuItem = new MenuItem(themeName, "", () => ThemeManager.ApplyTheme(themeName));
            themeMenuItems.Add(themeMenuItem);
        }

        showUnreadCountMenuItem = new MenuItem(
            settings.ShowUnreadCount ? "Hide _unread count" : "Show _unread count",
            "",
            ToggleShowUnreadCount);

        var menu = new MenuBar(
        [
            new MenuBarItem("_Mail",
            [
                new MenuItem("_New", "", null),
                new MenuItem("_Refresh", "", null),
                new MenuItem("_Quit", "", () => { Application.RequestStop(); })
            ]),
            new MenuBarItem("_Edit",
            [
                new MenuItem("_Copy", "", null),
                new MenuItem("C_ut", "", null),
                new MenuItem("_Paste", "", null)
            ]),
            new MenuBarItem("_View",
            [
                showUnreadCountMenuItem,
                new MenuItem("Hide _Mailboxes", "", null),
                new MenuItem("Hide _Preview", "", null),
                null,
                new MenuBarItem("_Theme", themeMenuItems.ToArray()),
            ])
        ]);
        Application.Top.Add(menu);
        Width = Dim.Fill();
        Height = Dim.Fill() - 1; // Leave one row for the status bar

        statusBar = new StatusBar(
        [
            new StatusItem(Key.Null, "~Ctrl-N~ New Message", null),
            new StatusItem(Key.Null, "~Ctrl-R~ Reload", null),
            new StatusItem(Key.Null, "~Ctrl-Q~ Quit", null)
        ]);
        Application.Top.Add(statusBar);

        mailboxesListView = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Sized(25),
            Height = Dim.Fill(),
        };
        Add(mailboxesListView);

        UpdateMailboxesList();

        mailboxesListView.SelectedItemChanged += (args) => LoadEmailsForSelectedMailbox(clearExisting: true);

        var verticalLine = new LineView(Orientation.Vertical)
        {
            X = Pos.Right(mailboxesListView),
            Y = 0,
            Height = Dim.Fill(),
        };
        // Add(verticalLine);

        messagesView = new TableView()
        {
            X = Pos.Right(verticalLine) - 1, // !!!
            Y = 0 - 1, // !!!
            Width = Dim.Fill() + 1, // !!!
            Height = Dim.Percent(50),
            FullRowSelect = true,
        };
        messagesView.Style.ExpandLastColumn = false;
        messagesView.Style.AlwaysShowHeaders = true;
        Add(messagesView);

        var horizontalLine = new LineView(Orientation.Horizontal)
        {
            X = Pos.Right(mailboxesListView),
            Y = Pos.Bottom(messagesView),
            Width = Dim.Fill(),
        };
        Add(horizontalLine);
        Add(verticalLine); // render vertical separator after the view

        emailView = new EmailView()
        {
            X = Pos.Right(verticalLine),
            Y = Pos.Bottom(horizontalLine),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        Add(emailView);

        // Create a DataTable for the email list
        emailsDataTable = new DataTable();
        emailsDataTable.Columns.Add("Time");
        emailsDataTable.Columns.Add("From");
        emailsDataTable.Columns.Add("Subject");
        emailsDataTable.Columns.Add("Body");
        emailsDataTable.Columns.Add("Unread", typeof(bool));

        messagesView.Table = emailsDataTable;

        messagesView.Style.GetOrCreateColumnStyle(messagesView.Table.Columns["Unread"]).Visible = false;

        var fromColumnStyle = messagesView.Style.GetOrCreateColumnStyle(messagesView.Table.Columns[1]);
        fromColumnStyle.MaxWidth = 30;
        var bodyColumnStyle = messagesView.Style.GetOrCreateColumnStyle(messagesView.Table.Columns[3]);
        bodyColumnStyle.Visible = false;

        messagesView.SelectedCellChanged += (args) =>
        {
            var row = args.NewRow;
            if (row >= 0 && row < emailsDataTable.Rows.Count)
            {
                var from = emailsDataTable.Rows[row]["From"].ToString()!;
                var subject = emailsDataTable.Rows[row]["Subject"].ToString()!;
                var body = emailsDataTable.Rows[row]["Body"].ToString()!;
                emailView.SetEmail(from, subject, body);
            }

            // Load next page when scrolling to the last email
            if (row == emailsDataTable.Rows.Count - 1 && nextPageToken != null)
            {
                LoadEmailsForSelectedMailbox(clearExisting: false);
            }
        };

        // Select the first row by default to display an email
        messagesView.SelectedRow = 0;
        messagesView.SetNeedsDisplay();

        ThemeManager.ThemeChanged += OnThemeChanged;

        // Load emails for the first mailbox
        if (mailboxes != null && mailboxes.Any())
        {
            mailboxesListView!.SelectedItem = 0;
            LoadEmailsForSelectedMailbox(clearExisting: true);
        }
    }

    private void ToggleShowUnreadCount()
    {
        settings.ShowUnreadCount = !settings.ShowUnreadCount;
        settingsService.SaveSettings(settings);
        if (showUnreadCountMenuItem != null)
        {
            showUnreadCountMenuItem.Title = settings.ShowUnreadCount ? "Hide _unread count" : "Show _unread count";
        }
        UpdateMailboxesList();
    }

    private void UpdateMailboxesList()
    {
        mailboxesListView.SetSource(mailboxes.Select(m =>
            settings.ShowUnreadCount && m.UnreadMessages > 0 ? $"{m.Name} ({m.UnreadMessages})" : m.Name).ToList());
    }

    private async void LoadEmailsForSelectedMailbox(bool clearExisting)
    {
        if (mailboxesListView.SelectedItem < 0 || mailboxesListView.SelectedItem >= mailboxes.Count)
            return;

        if (clearExisting)
        {
            nextPageToken = null;
            Application.MainLoop.Invoke(() =>
            {
                emailsDataTable.Rows.Clear();
                messagesView.SetNeedsDisplay();
                emailView.SetEmail("", "", ""); // Clear the current message
            });
        }

        Application.MainLoop.Invoke(() =>
        {
            if (!statusBar.Items.Any(item => item.Title.ToString() == "Loading messages..."))
            {
                statusBar.Items = statusBar.Items.Append(new StatusItem(Key.Null, "Loading messages...", null)).ToArray();
                // statusBar.SetNeedsDisplay();
            }
        });

        var selectedMailbox = mailboxes[mailboxesListView.SelectedItem];
        var emailListResult = await emailService.GetEmailsAsync(selectedMailbox.Id, nextPageToken);
        nextPageToken = emailListResult.NextPageToken;

        Application.MainLoop.Invoke(() =>
        {
            statusBar.Items = statusBar.Items.Where(item => item.Title.ToString() != "Loading messages...").ToArray();

            foreach (var email in emailListResult.Emails)
            {
                var displayTime = email.ReceivedDateTime.Date == DateTime.Today ?
                    email.ReceivedDateTime.ToShortTimeString() :
                    email.ReceivedDateTime.ToShortDateString();
                var fromText = email.IsUnread ? "* " + email.From : email.From;
                emailsDataTable.Rows.Add(displayTime, fromText, email.Subject, email.Snippet, email.IsUnread);
            }
            if (emailsDataTable.Rows.Count > 0)
            {
                if (clearExisting)
                {
                    messagesView.SelectedRow = 0;
                }
                else
                {
                    // After loading a new page, move the selection to the first new message
                    messagesView.SelectedRow = emailsDataTable.Rows.Count - emailListResult.Emails.Count;
                    messagesView.EnsureSelectedCellIsVisible();
                }
            }
            messagesView.SetNeedsDisplay();
        });
    }

    private void OnThemeChanged(string themeName)
    {
        foreach (var menuItem in themeMenuItems)
        {
            menuItem.Checked = menuItem.Title.ToString() == themeName;
        }
    }
}