using NoverApp.Config;
using NoverApp.Core;
using NoverApp.Data;
using NoverApp.Services;

namespace NoverApp.Web;

// Routen fuer Fragen, Bewertungen, Neues Gespraech und Statistik.
public class ChatHandler(UserService users, ChatService chats, ChatRepository repo)
{
    public async Task<(int Status, object Body)> SendeAsync(string name, ChatRequest? r)
    {
        if (r is null || string.IsNullOrWhiteSpace(r.Frage)) return (400, new { fehler = "Frage fehlt." });

        var kosten = TokenRules.Kosten(r.Schwer);
        if (users.Guthaben(name) < kosten)
            return (402, new { fehler = $"Zu wenig Tokens. Du brauchst {kosten}.", verbleibend = users.Guthaben(name) });

        var (ok, text, index) = await chats.SendeAsync(name, r.Frage.Trim(), r.Schwer);
        if (!ok) return (502, new { fehler = text, verbleibend = users.Guthaben(name) });

        return (200, new
        {
            antwort = text,
            index,
            verbleibend = users.Guthaben(name),
            modell = r.Schwer ? AppSettings.AnzeigeKomplex : AppSettings.AnzeigeSchnell,
        });
    }

    public (int Status, object Body) Bewerte(string name, RatingRequest? r)
    {
        if (r is null || (r.Wert != 1 && r.Wert != -1)) return (400, new { fehler = "Ungültige Bewertung." });
        repo.Bewerte(r.Index, r.Wert, name);
        return (200, new { nachricht = "Danke!" });
    }

    public (int Status, object Body) Feedback(string name, FeedbackRequest? r)
    {
        if (r is null || string.IsNullOrWhiteSpace(r.Text))
            return (400, new { fehler = "Schreib kurz, was falsch war." });
        repo.Feedback(r.Index, r.Text.Trim(), name);
        return (200, new { nachricht = "Danke, ich habe dein Feedback gespeichert. 🙌" });
    }

    public (int Status, object Body) Neu(string name)
    {
        chats.Neu(name);
        return (200, new { nachricht = "Neues Gespräch." });
    }

    public (int Status, object Body) Statistik(string name)
    {
        var eigene = repo.Von(name);
        var bewertet = eigene.Where(c => c.Bewertung is 1 or -1).ToList();
        int? zufrieden = bewertet.Count == 0
            ? null
            : (int)Math.Round(100.0 * bewertet.Count(c => c.Bewertung == 1) / bewertet.Count);
        int schwer = eigene.Count == 0
            ? 0
            : (int)Math.Round(100.0 * eigene.Count(c => c.Schwer) / eigene.Count);
        var letzte = eigene.AsEnumerable().Reverse().Take(10).Select(c => c.Frage).ToList();
        var tage = Enumerable.Range(0, 7)
            .Select(i => DateTime.Today.AddDays(i - 6))
            .Select(tag => new
            {
                tag = tag.ToString("dd.MM."),
                anzahl = eigene.Count(c => DateTime.TryParse(c.Zeit, out var z) && z.Date == tag.Date),
            })
            .ToList();

        return (200, new
        {
            gespraeche = eigene.Count,
            zufrieden,
            schwer,
            tokens = users.Guthaben(name),
            letzte,
            tage,
        });
    }
}
