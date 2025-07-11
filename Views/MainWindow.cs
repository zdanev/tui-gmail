using Terminal.Gui;

public class MainWindow : Window
{
    public MainWindow() : base("TUI Gmail")
    {
        X = 0;
        Y = 1; // Leave one row for the top-level menu

        var menu = new MenuBar (new MenuBarItem [] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_New", "", null),
                new MenuItem ("_Open", "", null),
                new MenuItem ("_Close", "", null),
                new MenuItem ("_Save", "", null),
                new MenuItem ("_Quit", "", () => { Application.RequestStop (); })
            }),
            new MenuBarItem ("_Edit", new MenuItem [] {
                new MenuItem ("_Copy", "", null),
                new MenuItem ("C_ut", "", null),
                new MenuItem ("_Paste", "", null)
            }),
            new MenuBarItem ("_View", new MenuItem [] {
                new MenuItem ("_Zoom In", "", null),
                new MenuItem ("Zoom _Out", "", null)
            })
        });
        Application.Top.Add (menu);
        Width = Dim.Fill();
        Height = Dim.Fill() - 1; // Leave one row for the status bar

        var statusBar = new StatusBar (new StatusItem [] {
            new StatusItem(Key.F1, "~F1~ Help", null),
            new StatusItem(Key.F2, "~F2~ Save", null),
            new StatusItem(Key.F3, "~F3~ Load", null)
        });
        Application.Top.Add (statusBar);

        var MailboxesListView = new ListView()
        {
            X = 0,
            Y = 0,
            Width = 25,
            Height = Dim.Fill(),
            // Border = new Border()
            // {
            //     Title = "Mailboxes",
            //     BorderStyle = Terminal.Gui.BorderStyle.Single
            // },
        };
        MailboxesListView.SetSource(new List<string> () { "Inbox", "Sent", "Drafts", "Trash" });
        Add(MailboxesListView);

        // var verticalLine = new LineView(Orientation.Vertical)
        // {
        //     X = Pos.Right(MailboxesListView),
        //     Y = 0,
        //     Height = Dim.Fill(),
        //     // Width = 1
        // };
        // Add(verticalLine);
    }
}
