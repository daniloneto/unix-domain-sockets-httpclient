# Unix Domain Sockets HttpClient

HttpClient sobre Unix Domain Sockets para .NET. Baixa latência intra-host, zero dependências, pronto para AOT/Trimming.

```
var client = UnixHttpClientFactory.For("/sockets/app.sock", TimeSpan.FromSeconds(5));
var pong = await client.GetFromJsonAsync<PingDto>("/ping", TestJsonContext.Default.PingDto);
```

Requisitos: Linux; em Windows lança NotSupportedException.

Veja exemplos de configuração do servidor Kestrel com sockets Unix no diretório `samples`.

Distribuído sob licença MIT.
