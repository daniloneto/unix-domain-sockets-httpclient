# unix-domain-sockets-httpclient

Cliente HTTP para Linux que usa Unix Domain Sockets (UDS) por baixo do HttpClient, reduzindo latência e overhead de loopback/TCP entre processos no mesmo host.

Implementado com SocketsHttpHandler.ConnectCallback, compatível com AOT/Trimming e System.Text.Json com source‑generation.
