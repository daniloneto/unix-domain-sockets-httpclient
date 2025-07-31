using UnixDomainSockets.HttpClient;
using System.Net.Http.Json;

var socketPath = "/tmp/uds-sample.sock";
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(opts =>
{
    opts.ListenUnixSocket(socketPath);
});

var app = builder.Build();

app.MapGet("/time", () => Results.Json(new { time = DateTimeOffset.UtcNow }));
app.MapPost("/sum", (int a, int b) => Results.Json(new { sum = a + b }));

app.Lifetime.ApplicationStarted.Register(async () =>
{
    if (!OperatingSystem.IsWindows())
    {
        File.SetUnixFileMode(socketPath, UnixFileMode.UserRead | UnixFileMode.UserWrite |
            UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.OtherRead | UnixFileMode.OtherWrite);
    }

    using var client = UnixHttpClientFactory.For(socketPath);
    var time = await client.GetFromJsonAsync<object>("/time");
    Console.WriteLine($"Time: {time}");
});

await app.RunAsync();
