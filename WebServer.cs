using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AI;

internal class WebServer
{
    byte[] image = new byte[] { 0x00 };

    public WebServer()
    {
        WebHost.CreateDefaultBuilder()
            .UseKestrel(k =>
            {
                k.ListenAnyIP(80);
            })
            .UseStartup<WebStartup>()
            .ConfigureLogging(cl =>
            {
                cl.ClearProviders();
            })
            .ConfigureServices(s =>
            {
                s.AddSingleton(this);
            })
            .Build()
            .RunAsync();
    }

    internal async Task ProcessHttpContext(HttpContext context)
    {
        var path = context.Request.Path.Value
            .Replace('\\', '/')
            .Trim('/', '\\').ToLower();

        if (path.Contains("image"))
        {
            context.Response.ContentType = "image/jpeg";
            await context.Response.Body.WriteAsync(image, 0, image.Length);
        }
        else
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(Html);
        }
    }

    public void SendImageUpdate(byte[] pngBytes)
    {
        image = pngBytes;
    }

    const string Html = @"
<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>AI</title>
    
    <style>
        body, html {
            background-color:#000;
            margin: 0;
            height: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        img {
            max-width: 100vw;
            height: auto; /* Maintain aspect ratio */
        }
    </style>

    <script>
        function updateImage() {
            document.getElementById(""image"").src = ""/image?time=""+new Date().getTime();
        }
        setInterval(updateImage, 250);
    </script>
</head>

<body>
    <img id=""image"" src=""/image"" />
</body>

</html>
";
}

internal class WebStartup
{
    public void Configure(IApplicationBuilder app, WebServer server)
    {
        app.UseWebSockets();
        app.UseWebServer(server);
    }
}

internal static class WebExtensions
{
    internal static IApplicationBuilder UseWebServer(
        this IApplicationBuilder app,
        WebServer server) =>
            app.Use((HttpContext c, Func<Task> _) =>
                server.ProcessHttpContext(c));
}
