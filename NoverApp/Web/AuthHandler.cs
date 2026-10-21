using NoverApp.Config;
using NoverApp.Core;
using NoverApp.Services;

namespace NoverApp.Web;

// Routen fuer Konto, Anmeldung und Abmeldung.
public class AuthHandler(UserService users)
{
    public (int Status, object Body) Registriere(RegisterRequest? r)
    {
        if (r is null) return Ungueltig();
        var (ok, meldung) = users.Registriere(r.Name, r.Passwort);
        if (!ok) return (400, new { fehler = meldung });
        return (200, new { nachricht = meldung });
    }

    public (int Status, object Body) Anmelden(LoginRequest? r)
    {
        if (r is null) return Ungueltig();
        var (ok, token, meldung) = users.Anmelden(r.Name, r.Passwort);
        if (!ok) return (401, new { fehler = meldung });
        return (200, new { token });
    }

    public (int Status, object Body) Abmelden(string? token)
    {
        if (token is not null) SessionStore.Remove(token);
        return (200, new { nachricht = "Abgemeldet." });
    }

    public (int Status, object Body) Me(string name)
    {
        var bonus = users.TagesBonus(name);
        var rolle = users.Rolle(name);
        return (200, new
        {
            name,
            rolle,
            anzeige = Roles.Display(rolle),
            tokens = users.Guthaben(name),
            bonus,
        });
    }

    private static (int Status, object Body) Ungueltig() => (400, new { fehler = "Ungültige Anfrage." });
}
