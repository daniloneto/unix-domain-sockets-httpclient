using System.Net.Http.Json;
using Xunit;

using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Http;
namespace UnixDomainSockets.HttpClient.Tests;

public class HttpClientFactoryTests : IClassFixture<UdsServerFixture>
{
    private readonly UdsServerFixture _fixture;

    public HttpClientFactoryTests(UdsServerFixture fixture)
    {
        _fixture = fixture;
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
