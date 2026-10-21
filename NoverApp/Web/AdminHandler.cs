using NoverApp.Core;

namespace NoverApp.Web;

// Fuehrt Befehle aus dem Chat aus (z.B. /tokens, /konto, /rolle).
public class AdminHandler(CommandParser parser)
{
    public (int Status, object Body) Befehl(string name, CommandRequest? r)
    {
        if (r is null || string.IsNullOrWhiteSpace(r.Text)) return (400, new { fehler = "Befehl fehlt." });
        return (200, new { antwort = parser.Ausfuehren(name, r.Text) });
    }
}
