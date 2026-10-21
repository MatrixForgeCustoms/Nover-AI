namespace NoverApp.Config;

// Rollen und ihre Rechte. Nur der Owner verwaltet Konten und Rollen.
public static class Roles
{
    public static readonly IReadOnlyDictionary<string, string> Namen = new Dictionary<string, string>
    {
        ["owner"] = "Owner",
        ["head_admin"] = "Head Admin",
        ["admin"] = "Admin",
        ["tester"] = "Tester",
        ["nutzer"] = "Nutzer",
    };

    public static bool IsValid(string rolle) => Namen.ContainsKey(rolle);

    public static bool IsAdmin(string rolle) => rolle is "owner" or "head_admin" or "admin";

    public static string Display(string rolle) => Namen.TryGetValue(rolle, out var n) ? n : "Nutzer";
}
