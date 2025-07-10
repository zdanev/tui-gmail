# TUI-Gmail

A simple Text User Interface (TUI) Gmail client.

## Getting Started

1.  **Clone the repository.**
2.  **Enable the Gmail API.**
    *   Go to the [Google Cloud Console](https://console.cloud.google.com/).
    *   Create a new project.
    *   Enable the Gmail API for your project.
    *   Create credentials for a **Desktop app**.
    *   Download the credentials as `credentials.json`.
3.  **Place `credentials.json` in the project root.**
    *   The application will look for a file named `credentials.json` in the same directory as the executable.
    *   You can use the `credentials.json.sample` file as a template.
4.  **Run the application.**
    *   The first time you run the application, you will be prompted to authenticate in your browser.
    *   After you authenticate, a `token.json` file will be created to store your credentials for future use.
