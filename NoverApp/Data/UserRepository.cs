using NoverApp.Config;

namespace NoverApp.Data;

// Zugriff auf nover_nutzer.json. Alle Zugriffe sind gesperrt, damit nichts gleichzeitig schreibt.
public class UserRepository
{
    private readonly object _lock = new();
    private readonly Dictionary<string, UserRecord> _users = Lade();

    private static Dictionary<string, UserRecord> Lade()
    {
        var daten = JsonStore.Load(AppSettings.UsersFile, new Dictionary<string, UserRecord>());
        foreach (var (schluessel, konto) in daten) konto.Name = schluessel;
        return daten;
    }

    public T Read<T>(Func<Dictionary<string, UserRecord>, T> f)
    {
        lock (_lock) return f(_users);
    }

    public T Modify<T>(Func<Dictionary<string, UserRecord>, T> f)
    {
        lock (_lock)
        {
            var ergebnis = f(_users);
            JsonStore.Save(AppSettings.UsersFile, _users);
            return ergebnis;
        }
    }
}
