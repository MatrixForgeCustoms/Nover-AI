using System.Net;
using NoverApp.Config;
using NoverApp.Core;

namespace NoverApp.Web;

// Lokaler HTTP-Server nur auf localhost. Liefert die Oberflaeche und die API.
public class ApiServer(int port, ApiRouter router)
{
    private readonly HttpListener _listener = new();
    private CancellationTokenSource? _cts;

    public void Start()
    {
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Start();
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => Schleife(_cts.Token));
        Logger.Info($"Server läuft auf http://localhost:{port}/");
    }

    private async Task Schleife(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await _listener.GetContextAsync();
            }
            catch (Exception)
            {
                break;
            }
            _ = Task.Run(() => BehandleAsync(ctx));
        }
    }

    private async Task BehandleAsync(HttpListenerContext ctx)
    {
        try
        {
            var pfad = ctx.Request.Url!.AbsolutePath;
            if (pfad.StartsWith("/api/", StringComparison.Ordinal)) await router.HandleAsync(ctx);
            else Statisch(ctx);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            try { Json.Send(ctx.Response, 500, new { fehler = "Interner Fehler." }); }
            catch (Exception) { /* Antwort bereits geschlossen */ }
        }
    }

    // Liefert Dateien aus wwwroot. Pfade ausserhalb von wwwroot werden blockiert.
    private static void Statisch(HttpListenerContext ctx)
    {
        var relativ = ctx.Request.Url!.AbsolutePath == "/"
            ? "index.html"
            : ctx.Request.Url.AbsolutePath.TrimStart('/');
        var basis = Path.GetFullPath(AppSettings.WwwRoot);
        var datei = Path.GetFullPath(Path.Combine(basis, relativ));

        if (!datei.StartsWith(basis, StringComparison.OrdinalIgnoreCase) || !File.Exists(datei))
        {
            ctx.Response.StatusCode = 404;
            ctx.Response.Close();
            return;
        }

        ctx.Response.ContentType = Path.GetExtension(datei).ToLowerInvariant() switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            _ => "application/octet-stream",
        };
        var bytes = File.ReadAllBytes(datei);
        ctx.Response.ContentLength64 = bytes.Length;
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.OutputStream.Close();
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener.Stop();
        _listener.Close();
    }
}
