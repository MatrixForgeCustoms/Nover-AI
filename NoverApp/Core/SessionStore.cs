using System.Security.Cryptography;
using NoverApp.Config;
using NoverApp.Data;

namespace NoverApp.Core;

// Sitzungen ("Angemeldet bleiben"). Jedes Token gilt 30 Tage.
public static class SessionStore
{
    public record SessionEntry(string Name, DateTime Ablauf);

    private static readonly object Lock = new();
    private static Dictionary<string, SessionEntry> _sitzungen =
        JsonStore.Load(AppSettings.SessionsFile, new Dictionary<string, SessionEntry>());

    public static string Create(string name)
    {
        lock (Lock)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            _sitzungen[token] = new SessionEntry(name, DateTime.UtcNow.AddDays(AppSettings.SitzungTage));
            Speichern();
            return token;
        }
    }

    public static string? Validate(string token)
    {
        lock (Lock)
        {
            if (!_sitzungen.TryGetValue(token, out var eintrag)) return null;
            if (eintrag.Ablauf < DateTime.UtcNow)
            {
                _sitzungen.Remove(token);
                Speichern();
                return null;
            }
            return eintrag.Name;
        }
    }

    public static void Remove(string token)
    {
        lock (Lock)
        {
            if (_sitzungen.Remove(token)) Speichern();
        }
    }

    private static void Speichern() => JsonStore.Save(AppSettings.SessionsFile, _sitzungen);
}
