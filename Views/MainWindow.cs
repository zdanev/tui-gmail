namespace TuiGmail.Views;

using System.Data;
using Terminal.Gui;
using Terminal.Gui.Graphs;
using TuiGmail.Services.Email;
using TuiGmail.Services.Infra;

public class MainWindow : Window
{
    private readonly IEmailService emailService;
    private readonly SettingsService settingsService;
    private readonly UserProfile userProfile;
    private readonly Settings settings;

    private IList<Mailbox>? mailboxes;
    private ListView? mailboxesListView;
    private DataTable? emailDataTable;
    private TableView? messagesView;
    private readonly List<MenuItem> themeMenuItems = new();
    private MenuItem? showUnreadCountMenuItem;
    private EmailView? emailView;

    public MainWindow(IEmailService emailService, SettingsService settingsService) : base("TUI Gmail")
    {
        this.emailService = emailService;
        this.settingsService = settingsService;
        this.settings = settingsService.LoadSettings();
        this.userProfile = emailService.GetUserProfile();

        InitializeComponent();
        ThemeManager.ThemeChanged += OnThemeChanged;

        // Load emails for the first mailbox
        if (mailboxes != null && mailboxes.Any())
        {
            mailboxesListView!.SelectedItem = 0;
            LoadEmailsForSelectedMailbox();
        }
    }

    private void OnThemeChanged(string themeName)
    {
        foreach (var menuItem in themeMenuItems)
        {
            menuItem.Checked = menuItem.Title.ToString() == themeName;
        }
    }

    private void InitializeComponent()
    {
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
            settings.ShowUnreadCount ? "Hide unread count" : "Show unread count",
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
                new MenuBarItem("_Theme", themeMenuItems.ToArray()),
                showUnreadCountMenuItem,
                new MenuItem("Hide _Mailboxes", "", null),
                new MenuItem("Hide _Preview", "", null)
            ])
        ]);
        Application.Top.Add(menu);
        Width = Dim.Fill();
        Height = Dim.Fill() - 1; // Leave one row for the status bar

        var statusBar = new StatusBar(new StatusItem[]
        {
            new StatusItem(Key.F1, "~F1~ Help", null),
            new StatusItem(Key.F2, "~F2~ Save", null),
            new StatusItem(Key.F3, "~F3~ Load", null)
        });
        Application.Top.Add(statusBar);

        mailboxesListView = new ListView()
        {
            X = 0,
            Y = 0,
            Width = 25,
            Height = Dim.Fill(),
        };
        Add(mailboxesListView);

        this.mailboxes = this.emailService.GetMailboxes();
        UpdateMailboxesList();

        mailboxesListView.SelectedItemChanged += (args) => LoadEmailsForSelectedMailbox();

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
        messagesView.Style.ExpandLastColumn = true;
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
        emailDataTable = new DataTable();
        emailDataTable.Columns.Add("Time");
        emailDataTable.Columns.Add("From");
        emailDataTable.Columns.Add("Subject");
        emailDataTable.Columns.Add("Body");

        messagesView.Table = emailDataTable;

        messagesView.SelectedCellChanged += (args) =>
        {
            var row = args.NewRow;
            if (row >= 0 && row < emailDataTable.Rows.Count)
            {
                var from = emailDataTable.Rows[row]["From"].ToString()!;
                var subject = emailDataTable.Rows[row]["Subject"].ToString()!;
                var body = emailDataTable.Rows[row]["Body"].ToString()!;
                emailView.SetEmail(from, subject, body);
            }
        };

        // Select the first row by default to display an email
        messagesView.SelectedRow = 0;
        messagesView.SetNeedsDisplay();
    }

    private void ToggleShowUnreadCount()
    {
        settings.ShowUnreadCount = !settings.ShowUnreadCount;
        settingsService.SaveSettings(settings);
        if (showUnreadCountMenuItem != null)
        {
            showUnreadCountMenuItem.Title = settings.ShowUnreadCount ? "Hide unread count" : "Show unread count";
        }
        UpdateMailboxesList();
    }

    private void UpdateMailboxesList()
    {
        if (mailboxesListView != null && mailboxes != null)
        {
            mailboxesListView.SetSource(mailboxes.Select(m =>
                settings.ShowUnreadCount && m.UnreadMessages > 0 ? $"{m.Name} ({m.UnreadMessages})" : m.Name).ToList());
        }
    }

    private async void LoadEmailsForSelectedMailbox()
    {
        if (mailboxesListView == null || mailboxes == null || mailboxesListView.SelectedItem < 0 || mailboxesListView.SelectedItem >= mailboxes.Count)
            return;

        // Clear messages and show loading indicator
        Application.MainLoop.Invoke(() =>
        {
            emailDataTable!.Rows.Clear();
            emailDataTable.Rows.Add("Loading", "messages", "...", "");
            messagesView!.SetNeedsDisplay();
            emailView!.SetEmail("", "", ""); // Clear the current message
        });

        var selectedMailbox = mailboxes[mailboxesListView.SelectedItem];
        var emails = await emailService.GetEmailsAsync(selectedMailbox.Id);

        Application.MainLoop.Invoke(() =>
        {
            emailDataTable!.Rows.Clear(); // Clear loading indicator
            foreach (var email in emails)
            {
                var displayTime = email.ReceivedDateTime.Date == DateTime.Today ?
                    email.ReceivedDateTime.ToShortTimeString() :
                    email.ReceivedDateTime.ToShortDateString();
                emailDataTable.Rows.Add(displayTime, email.From, email.Subject, email.Snippet);
            }
            if (emailDataTable.Rows.Count > 0)
            {
                messagesView!.SelectedRow = 0;
            }
            messagesView!.SetNeedsDisplay();
        });
    }
}