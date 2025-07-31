using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;

namespace UnixDomainSockets.HttpClient.Tests;

public sealed class UdsServerFixture : IAsyncLifetime
{
    public string SocketPath { get; } = $"/tmp/uds-httpclient-tests-{Guid.NewGuid():N}.sock";
    private IHost? _host;

    public async Task InitializeAsync()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    options.ListenUnixSocket(SocketPath);
                });
                webBuilder.Configure(app =>
                {
                    app.MapGet("/ping", () => Results.Json(new { ok = true }));
                    app.MapPost("/echo", async (HttpContext ctx) =>
                    {
                        var body = await ctx.Request.ReadFromJsonAsync<object>();
                        await ctx.Response.WriteAsJsonAsync(body);
                    });
                });
            });

        _host = builder.Build();
        await _host.StartAsync();

        // ensure other processes can access socket
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
            await _host.StopAsync();
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
