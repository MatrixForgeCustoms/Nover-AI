using NoverApp.Config;
using NoverApp.Core;
using NoverApp.Data;

namespace NoverApp.Services;

// Geschaeftslogik fuer Konten, Anmeldung, Tokens und Rollen.
public class UserService(UserRepository repo)
{
    public static string Norm(string name) => name.Trim().ToLowerInvariant();

    public (bool Ok, string Meldung) Registriere(string name, string passwort, string rolle = "nutzer")
    {
        name = Norm(name);
        if (name.Length < 3) return (false, "Der Name braucht mindestens 3 Zeichen.");
        if (passwort.Length < 4) return (false, "Das Passwort braucht mindestens 4 Zeichen.");
        if (!Roles.IsValid(rolle))
            return (false, "Unbekannte Rolle. Erlaubt: " + string.Join(", ", Roles.Namen.Keys));

        var salt = PasswordHasher.NewSalt();
        var hash = PasswordHasher.Hash(passwort, salt);
        return repo.Modify(d =>
        {
            if (d.ContainsKey(name)) return (false, "Den Namen gibt es schon.");
            d[name] = new UserRecord { Name = name, Salt = salt, Hash = hash, Rolle = rolle };
            Logger.Info($"Konto erstellt: {name} ({rolle})");
            return (true, "Konto erstellt.");
        });
    }

    public (bool Ok, string Token, string Meldung) Anmelden(string name, string passwort)
    {
        name = Norm(name);
        var konto = repo.Read<UserRecord?>(d => d.TryGetValue(name, out var u) ? u : null);
        if (konto is null || !PasswordHasher.Verify(passwort, konto.Salt, konto.Hash))
        {
            Logger.Warn($"Fehlgeschlagene Anmeldung: {name}");
            return (false, "", "Name oder Passwort falsch.");
        }
        Logger.Info($"Angemeldet: {name}");
        return (true, SessionStore.Create(name), "Angemeldet.");
    }

    // Gibt den Tagesbonus hoechstens einmal pro Kalendertag.
    public bool TagesBonus(string name)
    {
        var heute = DateTime.Now.ToString("yyyy-MM-dd");
        return repo.Modify(d =>
        {
            if (!d.TryGetValue(name, out var u) || u.BonusTag == heute) return false;
            u.Tokens += AppSettings.TagesBonus;
            u.BonusTag = heute;
            Logger.Info($"Tagesbonus +{AppSettings.TagesBonus} fuer {name}");
            return true;
        });
    }

    public int Guthaben(string name) =>
        repo.Read(d => d.TryGetValue(name, out var u) ? u.Tokens : 0);

    public bool Abbuchen(string name, int betrag) =>
        repo.Modify(d =>
        {
            if (!d.TryGetValue(name, out var u) || u.Tokens < betrag) return false;
            u.Tokens -= betrag;
            return true;
        });

    public void Gutschreiben(string name, int betrag)
    {
        repo.Modify(d =>
        {
            if (d.TryGetValue(name, out var u))
            {
                u.Tokens += betrag;
                Logger.Info($"Gutschrift {betrag:+#;-#;0} an {name}");
            }
            return true;
        });
    }

    public string Rolle(string name) =>
        repo.Read(d => d.TryGetValue(name, out var u) ? u.Rolle : "nutzer");

    public bool IstOwner(string name) => Rolle(name) == "owner";

    public bool Existiert(string name) => repo.Read(d => d.ContainsKey(Norm(name)));

    public (bool Ok, string Meldung) SetRolle(string name, string neueRolle)
    {
        name = Norm(name);
        if (name == AppSettings.OwnerName) return (false, "Die Rolle des Owners bleibt unverändert.");
        if (!Roles.IsValid(neueRolle))
            return (false, "Unbekannte Rolle. Erlaubt: " + string.Join(", ", Roles.Namen.Keys));

        return repo.Modify(d =>
        {
            if (!d.TryGetValue(name, out var u)) return (false, "Den Nutzer gibt es nicht.");
            u.Rolle = neueRolle;
            Logger.Info($"Rolle von {name} auf {neueRolle} gesetzt");
            return (true, $"{name} ist jetzt {Roles.Display(neueRolle)}.");
        });
    }

    // Gibt dem Owner-Konto die Owner-Rolle, falls sie fehlt.
    public void EnsureOwner()
    {
        repo.Modify(d =>
        {
            if (d.TryGetValue(AppSettings.OwnerName, out var u) && u.Rolle != "owner")
            {
                u.Rolle = "owner";
                Logger.Info("Owner-Rolle gesetzt");
            }
            return true;
        });
    }

    // Legt ein Konto fuer Google/GitHub an oder meldet das vorhandene an. Gibt den Kontonamen zurueck.
    public string OAuthAnmelden(string provider, string id, string email)
    {
        var name = $"{provider}-{id}";
        repo.Modify(d =>
        {
            if (!d.ContainsKey(name))
            {
                d[name] = new UserRecord { Name = name, Rolle = "nutzer", Provider = provider, Email = email };
                Logger.Info($"Konto ueber {provider} erstellt: {name}");
            }
            return true;
        });
        Logger.Info($"Angemeldet ueber {provider}: {name}");
        return name;
    }
}
