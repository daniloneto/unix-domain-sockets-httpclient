## ✨ Features

- 🔌 `UnixHttpClientFactory.For(...)` – Cria `HttpClient` usando Unix Domain Sockets via `SocketsHttpHandler.ConnectCallback`.
- 🧪 Pronto para testes e produção em sistemas Linux com suporte a UDS.
- 📦 Sem dependências externas (só BCL).
- ✅ Compatível com trimming e AOT (testado com PublishAot=true no sample).
- 🧪 Testes com Kestrel escutando via UDS.
- ⚠️ Lança `NotSupportedException` em Windows com mensagem clara.

## 🧰 Quickstart

```csharp
var client = UnixHttpClientFactory.For("/tmp/my-app.sock", TimeSpan.FromSeconds(5));
var response = await client.GetFromJsonAsync<PingDto>("/ping", MyJsonContext.Default.PingDto);