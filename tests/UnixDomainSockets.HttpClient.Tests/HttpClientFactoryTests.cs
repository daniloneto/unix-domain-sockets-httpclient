using System.Net.Http.Json;
using Xunit;

namespace UnixDomainSockets.HttpClient.Tests;

public class HttpClientFactoryTests : IClassFixture<UdsServerFixture>
{
    private readonly UdsServerFixture _fixture;

    public HttpClientFactoryTests(UdsServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Can_Get_Ping()
    {
        if (OperatingSystem.IsWindows())
        {
            await Assert.ThrowsAsync<NotSupportedException>(() => Task.Run(() => UnixHttpClientFactory.For(_fixture.SocketPath)));
            return;
        }

        using var client = UnixHttpClientFactory.For(_fixture.SocketPath, TimeSpan.FromSeconds(5));
        var result = await client.GetFromJsonAsync<PingDto>("/ping", TestJsonContext.Default.PingDto);
        Assert.NotNull(result);
        Assert.True(result!.ok);
    }

    [Fact]
    public async Task Can_Post_Echo()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        using var client = UnixHttpClientFactory.For(_fixture.SocketPath);
        var payload = new { message = "hi" };
        var response = await client.PostAsJsonAsync("/echo", payload);
        response.EnsureSuccessStatusCode();
        var echoed = await response.Content.ReadFromJsonAsync<object>(TestJsonContext.Default.Object);
        Assert.NotNull(echoed);
    }

    [Fact]
    public async Task Invalid_Path_Throws_HttpRequestException()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var invalidPath = "/tmp/does-not-exist.sock";
        using var client = UnixHttpClientFactory.For(invalidPath, TimeSpan.FromSeconds(1));
        await Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetAsync("/ping"));
    }
}
