using NoverApp.AI;
using NoverApp.Config;
using NoverApp.Core;
using NoverApp.Data;

namespace NoverApp.Services;

// Ein Gespraech: Tokens abbuchen, Kontext pflegen, Antwort holen und speichern.
public class ChatService(UserService users, ChatRepository chats, GeminiProvider gemini, OpenAiCompatibleProvider groq)
{
    private readonly Dictionary<string, List<(string Rolle, string Text)>> _kontext = new();
    private readonly object _lock = new();

    public async Task<(bool Ok, string Text, int Index)> SendeAsync(string name, string frage, bool schwer)
    {
        var kosten = TokenRules.Kosten(schwer);
        if (!users.Abbuchen(name, kosten))
            return (false, $"Zu wenig Tokens. Du brauchst {kosten}.", -1);

        List<(string Rolle, string Text)> verlauf;
        lock (_lock)
        {
            verlauf = _kontext.TryGetValue(name, out var bisher)
                ? new List<(string Rolle, string Text)>(bisher)
                : new List<(string Rolle, string Text)>();
        }
        verlauf.Add(("user", frage));

        var system = PromptBuilder.Baue(schwer);
        var (ok, text) = schwer
            ? await groq.FrageAsync(system, verlauf, AppSettings.ModellKomplex)
            : await gemini.FrageAsync(system, verlauf, AppSettings.ModellSchnell);

        if (!ok)
        {
            // Fehler: Tokens zurueck
            users.Gutschreiben(name, kosten);
            return (false, text, -1);
        }

        lock (_lock)
        {
            if (!_kontext.TryGetValue(name, out var liste))
            {
                liste = new List<(string Rolle, string Text)>();
                _kontext[name] = liste;
            }
            liste.Add(("user", frage));
            liste.Add(("assistant", text));
            while (liste.Count > AppSettings.KontextNachrichten) liste.RemoveAt(0);
        }

        var index = chats.Add(new ChatRecord
        {
            Zeit = DateTime.Now.ToString("o"),
            Benutzer = name,
            Frage = frage,
            Antwort = text,
            Kosten = kosten,
            Schwer = schwer,
        });
        return (true, text, index);
    }

    public void Neu(string name)
    {
        lock (_lock) _kontext.Remove(name);
    }
}
