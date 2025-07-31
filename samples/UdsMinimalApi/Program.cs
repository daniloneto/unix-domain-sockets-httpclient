using UnixDomainSockets.HttpClient;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace UdsMinimalApi
    
{    
    public record TimeResponse(DateTimeOffset time);
    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(TimeResponse))]  
    public partial class SampleJsonContext : JsonSerializerContext {
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var socketPath = "/tmp/uds-sample.sock";
            
            if (File.Exists(socketPath))
            {
                File.Delete(socketPath);
            }
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseKestrel(opts =>
            {
                opts.ListenUnixSocket(socketPath);
            });

            var app = builder.Build();
       
            app.MapGet("/time", () => Results.Json(new TimeResponse(DateTimeOffset.UtcNow), SampleJsonContext.Default.TimeResponse));

            app.Lifetime.ApplicationStarted.Register(async () =>
            {
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(socketPath, UnixFileMode.UserRead | UnixFileMode.UserWrite |
                        UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.OtherRead | UnixFileMode.OtherWrite);
                }

                using var client = UnixHttpClientFactory.For(socketPath);
                var time = await client.GetFromJsonAsync<TimeResponse>("/time", SampleJsonContext.Default.TimeResponse);
                Console.WriteLine($"Time: {time}");
            });

            await app.RunAsync();
        }
    }
}

