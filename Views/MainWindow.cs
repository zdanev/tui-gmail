namespace TuiGmail.Views;

using System.Data;
using Terminal.Gui;
using Terminal.Gui.Graphs;

using TuiGmail.EmailService;

public class MainWindow : Window
{
    private readonly IEmailService emailService;
    private readonly UserProfile userProfile;

    private DataTable? emailDataTable;
    private MenuItem defaultTheme = new MenuItem("Default", "", null) { Checked = true };
    private MenuItem darkTheme = new MenuItem("Dark", "", null);
    private MenuItem lightTheme = new MenuItem("Light", "", null);
    private MenuItem darkOrangeTheme = new MenuItem("Dark Orange", "", null);
    private Label? userEmailLabel;
    private ListView? mailboxesListView;
    private TableView? messagesView;

    public MainWindow(IEmailService emailService) : base("TUI Gmail")
    {
        this.emailService = emailService;
        this.userProfile = emailService.GetUserProfile();
        this.Title = userProfile.EmailAddress;

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        X = 0;
        Y = 1; // Leave one row for the top-level menu

        defaultTheme.Action = () =>
        {
            ThemeManager.ApplyTheme(ThemeManager.DefaultScheme, defaultTheme);
        };

        darkTheme.Action = () =>
        {
            ThemeManager.ApplyTheme(ThemeManager.DarkScheme, darkTheme);
        };

        lightTheme.Action = () =>
        {
            ThemeManager.ApplyTheme(ThemeManager.LightScheme, lightTheme);
        };

        darkOrangeTheme.Action = () =>
        {
            ThemeManager.ApplyTheme(ThemeManager.DarkOrangeScheme, darkOrangeTheme);
        };

        var menu = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_File", new MenuItem[]
            {
                new MenuItem("_New", "", null),
                new MenuItem("_Open", "", null),
                new MenuItem("_Close", "", null),
                new MenuItem("_Save", "", null),
                new MenuItem("_Quit", "", () => { Application.RequestStop(); })
            }),
            new MenuBarItem("_Edit", new MenuItem[]
            {
                new MenuItem("_Copy", "", null),
                new MenuItem("C_ut", "", null),
                new MenuItem("_Paste", "", null)
            }),
            new MenuBarItem("_View", new MenuItem[]
            {
                new MenuBarItem("_Theme", new MenuItem[] { defaultTheme, darkTheme, lightTheme, darkOrangeTheme }),
                new MenuItem("_Zoom In", "", null),
                new MenuItem("Zoom _Out", "", null)
            })
        });
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

        userEmailLabel = new Label("Loading user...")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
            TextAlignment = TextAlignment.Centered,
        };
        Add(userEmailLabel);

        mailboxesListView = new ListView()
        {
            X = 0,
            Y = Pos.Bottom(userEmailLabel),
            Width = 25,
            Height = Dim.Fill(),
        };
        Add(mailboxesListView);

        var mailboxes = this.emailService.GetMailboxes();
        mailboxesListView.SetSource(mailboxes.Select(m => m.Name).ToList());

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

        var emailView = new EmailView()
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

        // Add some dummy data
        emailDataTable.Rows.Add("10:00 AM", "sender1@example.com", "Hello World", "This is the body of the first email.");
        emailDataTable.Rows.Add("10:00 AM", "sender2@example.com", "Re: Your order", "This is the body of the second email.");
        emailDataTable.Rows.Add("Yesterday", "sender3@example.com", "Important update", "This is the body of the third email.");

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
}