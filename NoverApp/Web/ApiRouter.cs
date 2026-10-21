using System.Net;
using NoverApp.Core;

namespace NoverApp.Web;

// Ordnet jede Route einem Handler zu. Geschuetzte Routen brauchen ein gueltiges Token.
public class ApiRouter(AuthHandler auth, ChatHandler chat, AdminHandler admin)
{
    public async Task HandleAsync(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var pfad = req.Url!.AbsolutePath;
        var token = Json.Bearer(req);
        var name = token is null ? null : SessionStore.Validate(token);

        var (status, body) = (req.HttpMethod, pfad) switch
        {
            ("POST", "/api/register") => auth.Registriere(Json.Read<RegisterRequest>(req)),
            ("POST", "/api/login") => auth.Anmelden(Json.Read<LoginRequest>(req)),
            ("POST", "/api/logout") => auth.Abmelden(token),
            ("GET", "/api/me") when name is not null => auth.Me(name),
            ("POST", "/api/chat") when name is not null => await chat.SendeAsync(name, Json.Read<ChatRequest>(req)),
            ("POST", "/api/rating") when name is not null => chat.Bewerte(name, Json.Read<RatingRequest>(req)),
            ("POST", "/api/feedback") when name is not null => chat.Feedback(name, Json.Read<FeedbackRequest>(req)),
            ("POST", "/api/new") when name is not null => chat.Neu(name),
            ("GET", "/api/stats") when name is not null => chat.Statistik(name),
            ("POST", "/api/command") when name is not null => admin.Befehl(name, Json.Read<CommandRequest>(req)),
            _ when pfad.StartsWith("/api/", StringComparison.Ordinal) && name is null
                => (401, (object)new { fehler = "Bitte anmelden." }),
            _ => (404, (object)new { fehler = "Unbekannte Route." }),
        };

        Json.Send(ctx.Response, status, body);
    }
}
