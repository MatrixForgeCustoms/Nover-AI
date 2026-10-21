using NoverApp.Config;

namespace NoverApp.Data;

// Zugriff auf nover_daten.json (alle Gespraeche).
public class ChatRepository
{
    private readonly object _lock = new();
    private readonly List<ChatRecord> _chats =
        JsonStore.Load(AppSettings.ChatsFile, new List<ChatRecord>());

    public int Add(ChatRecord chat)
    {
        lock (_lock)
        {
            _chats.Add(chat);
            Speichern();
            return _chats.Count - 1;
        }
    }

    // Bewertung nur durch den Benutzer, dem das Gespraech gehoert.
    public void Bewerte(int index, int wert, string benutzer)
    {
        lock (_lock)
        {
            if (index < 0 || index >= _chats.Count) return;
            if (_chats[index].Benutzer != benutzer) return;
            _chats[index].Bewertung = wert;
            Speichern();
        }
    }

    // Freitext-Rueckmeldung, was falsch war und wie es besser waere.
    public void Feedback(int index, string text, string benutzer)
    {
        lock (_lock)
        {
            if (index < 0 || index >= _chats.Count) return;
            if (_chats[index].Benutzer != benutzer) return;
            _chats[index].Feedback = text;
            Speichern();
        }
    }

    public List<ChatRecord> Von(string benutzer)
    {
        lock (_lock)
        {
            return _chats.Where(c => c.Benutzer == benutzer).ToList();
        }
    }

    private void Speichern() => JsonStore.Save(AppSettings.ChatsFile, _chats);
}
