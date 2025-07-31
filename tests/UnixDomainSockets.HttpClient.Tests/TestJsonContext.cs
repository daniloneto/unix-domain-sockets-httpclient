using System.Text.Json.Serialization;

namespace UnixDomainSockets.HttpClient.Tests;

[JsonSerializable(typeof(PingDto))]
[JsonSerializable(typeof(object))]
public partial class TestJsonContext : JsonSerializerContext
{
}

public record PingDto(bool ok);
