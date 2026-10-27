using System.Net;
using System.Text;
using NoverApp.Core;
using NoverApp.Services;

namespace NoverApp.Web;

// Routen: /api/oauth/{google|github}/{start|callback}
public class OAuthHandler(OAuthService oauth, UserService users)
{
    public async Task HandleAsync(HttpListenerContext ctx)
    {
        var teile = ctx.Request.Url!.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (teile.Length != 4 || teile[2] is not ("google" or "github"))
        {
            Text(ctx, 404, "Unbekannte Anmeldung.");
            return;
        }
        var provider = teile[2];

        if (teile[3] == "start")
        {
            var url = oauth.Start(provider);
            if (url is null)
            {
                Text(ctx, 503, $"Die Anmeldung mit {provider} ist noch nicht eingerichtet.");
                return;
            }
            Redirect(ctx, url);
            return;
        }

        if (teile[3] == "callback")
        {
            var code = ctx.Request.QueryString["code"];
            var state = ctx.Request.QueryString["state"];
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state) || !oauth.CheckState(state, provider))
            {
                Text(ctx, 400, "Anmeldung abgebrochen oder ungültig. Bitte erneut versuchen.");
                return;
            }

            var nutzer = await oauth.FinishAsync(provider, code);
            if (nutzer is null)
            {
                Text(ctx, 502, "Die Anmeldung beim Anbieter ist fehlgeschlagen.");
                return;
            }

            var name = users.OAuthAnmelden(provider, nutzer.Id, nutzer.Email);
            var token = SessionStore.Create(name);
            Redirect(ctx, "/?token=" + Uri.EscapeDataString(token));
            return;
        }

        Text(ctx, 404, "Unbekannte Anmeldung.");
    }

    private static void Redirect(HttpListenerContext ctx, string url)
    {
        ctx.Response.StatusCode = 302;
        ctx.Response.Headers["Location"] = url;
        ctx.Response.Close();
    }

    private static void Text(HttpListenerContext ctx, int status, string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "text/plain; charset=utf-8";
        ctx.Response.ContentLength64 = bytes.Length;
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.OutputStream.Close();
    }
}
