using NoverApp.Config;
using NoverApp.Services;

namespace NoverApp.Core;

// Verarbeitet Befehle wie /hilfe, /tokens, /konto und /rolle.
public class CommandParser(UserService users, ChatService chats)
{
    private const string Hilfe =
        "Befehle:\n" +
        "/hilfe - diese Liste\n" +
        "/guthaben - zeigt deine Tokens\n" +
        "/neu - neues Gespräch\n" +
        "/tokens <name> <betrag> - Admins\n" +
        "/konto <name> <passwort> <rolle> - nur Owner\n" +
        "/rolle <name> <rolle> - nur Owner";

    public string Ausfuehren(string name, string text)
    {
        var teile = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (teile.Length == 0) return "";

        return teile[0].ToLowerInvariant() switch
        {
            "/hilfe" => Hilfe,
            "/guthaben" => $"Du hast {users.Guthaben(name)} Tokens.",
            "/neu" => Neu(name),
            "/abmelden" => "Nutze den Abmelden-Knopf in der Seitenleiste.",
            "/tokens" => Tokens(name, teile),
            "/konto" => Konto(name, teile),
            "/rolle" => Rolle(name, teile),
            _ => $"Unbekannter Befehl: {teile[0]}. Tippe /hilfe.",
        };
    }

    private string Neu(string name)
    {
        chats.Neu(name);
        return "Neues Gespräch gestartet.";
    }

    private string Tokens(string name, string[] teile)
    {
        if (!Roles.IsAdmin(users.Rolle(name))) return "Dieser Befehl ist nur für Admins.";
        if (teile.Length != 3 || !int.TryParse(teile[2], out var betrag))
            return "Benutzung: /tokens <name> <betrag>";
        var ziel = UserService.Norm(teile[1]);
        if (!users.Existiert(ziel)) return "Den Nutzer gibt es nicht.";
        users.Gutschreiben(ziel, betrag);
        return $"{ziel} hat jetzt {users.Guthaben(ziel)} Tokens.";
    }

    private string Konto(string name, string[] teile)
    {
        if (!users.IstOwner(name)) return "Dieser Befehl ist nur für den Owner.";
        if (teile.Length != 4)
            return "Benutzung: /konto <name> <passwort> <rolle>\nRollen: " + string.Join(", ", Roles.Namen.Keys);
        var rolle = teile[3].ToLowerInvariant();
        var (ok, meldung) = users.Registriere(teile[1], teile[2], rolle);
        return ok ? $"Konto {UserService.Norm(teile[1])} als {Roles.Display(rolle)} erstellt." : meldung;
    }

    private string Rolle(string name, string[] teile)
    {
        if (!users.IstOwner(name)) return "Dieser Befehl ist nur für den Owner.";
        if (teile.Length != 3) return "Benutzung: /rolle <name> <rolle>";
        return users.SetRolle(teile[1], teile[2].ToLowerInvariant()).Meldung;
    }
}
