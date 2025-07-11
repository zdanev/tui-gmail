namespace TuiGmail.Views;

using Terminal.Gui;

public class EmailView : View
{
    private Label fromLabel;
    private Label subjectLabel;
    private TextView bodyTextView;

    public EmailView()
    {
        // Layout for the EmailView itself
        Width = Dim.Fill();
        Height = Dim.Fill();

        // From Label
        fromLabel = new Label("From: ")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
        };
        Add(fromLabel);

        // Subject Label
        subjectLabel = new Label("Subject: ")
        {
            X = 0,
            Y = Pos.Bottom(fromLabel),
            Width = Dim.Fill(),
            Height = 1,
        };
        Add(subjectLabel);

        // Body TextView
        bodyTextView = new TextView()
        {
            X = 0,
            Y = Pos.Bottom(subjectLabel),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
        };
        Add(bodyTextView);
    }

    public void SetEmail(string from, string subject, string body)
    {
        fromLabel.Text = $"From: {from}";
        subjectLabel.Text = $"Subject: {subject}";
        bodyTextView.Text = body;
    }
}