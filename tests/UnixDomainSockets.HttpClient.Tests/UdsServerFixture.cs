

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
// Removendo o using de Kestrel que não existe para net9.0

namespace UnixDomainSockets.HttpClient.Tests
{
// Removendo IAsyncLifetime, pois não está disponível no contexto de testes
public sealed class UdsServerFixture
    {
        public string SocketPath { get; } = $"/tmp/uds-httpclient-tests-{Guid.NewGuid():N}.sock";
        private IWebHost? _host;

        public async Task InitializeAsync()
        {
            var builder = new WebHostBuilder()
                .UseUrls($"http://unix:{SocketPath}")
                .Configure(app =>
                {
                    app.Use(async (ctx, next) =>
                    {
                        if (ctx.Request.Path == "/ping" && ctx.Request.Method == "GET")
                        {
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsync("{\"ok\":true}");
                            return;
                        }
                        if (ctx.Request.Path == "/echo" && ctx.Request.Method == "POST")
                        {
                            using var reader = new StreamReader(ctx.Request.Body);
                            var bodyStr = await reader.ReadToEndAsync();
                            var body = System.Text.Json.JsonSerializer.Deserialize<object>(bodyStr);
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(body));
                            return;
                        }
                        await next();
                    });
                });

            _host = builder.Build();
            await Task.Run(() => _host.Start());

            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(SocketPath, UnixFileMode.UserRead | UnixFileMode.UserWrite |
                    UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.OtherRead | UnixFileMode.OtherWrite);
            }
        }

        public async Task DisposeAsync()
        {
            if (_host is not null)
            {
                await Task.Run(() => _host.StopAsync());
                _host.Dispose();
            }

            try
            {
                File.Delete(SocketPath);
            }
            catch
            {
            }
        }
    }
}
