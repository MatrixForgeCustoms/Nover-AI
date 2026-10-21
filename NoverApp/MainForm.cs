using Microsoft.Web.WebView2.WinForms;
using NoverApp.AI;
using NoverApp.Config;
using NoverApp.Core;
using NoverApp.Data;
using NoverApp.Services;
using NoverApp.Web;

namespace NoverApp;

// Hauptfenster: startet den lokalen Server und zeigt die Oberflaeche in WebView2.
public class MainForm : Form
{
    private readonly WebView2 _web = new() { Dock = DockStyle.Fill };
    private ApiServer? _server;

    public MainForm()
    {
        Text = "Nover";
        Width = 1140;
        Height = 780;
        MinimumSize = new Size(860, 580);
        BackColor = Color.FromArgb(10, 10, 10);
        Controls.Add(_web);
        Load += async (_, _) => await StarteAsync();
        FormClosed += (_, _) => _server?.Stop();
    }

    private async Task StarteAsync()
    {
        _server = Baue();
        _server.Start();
        await _web.EnsureCoreWebView2Async();
        _web.CoreWebView2.Settings.AreDevToolsEnabled = false;
        _web.Source = new Uri($"http://localhost:{AppSettings.Port}/");
    }

    // Alle Dienste hier verbinden (einfache Abhaengigkeitsverwaltung)
    private static ApiServer Baue()
    {
        var users = new UserService(new UserRepository());
        users.EnsureOwner();
        var chatRepo = new ChatRepository();
        var groq = new OpenAiCompatibleProvider("Groq", AppSettings.GroqUrl, "GROQ_API_KEY");
        var chats = new ChatService(users, chatRepo, new GeminiProvider(), groq);
        var router = new ApiRouter(
            new AuthHandler(users),
            new ChatHandler(users, chats, chatRepo),
            new AdminHandler(new CommandParser(users, chats)));
        return new ApiServer(AppSettings.Port, router);
    }
}
