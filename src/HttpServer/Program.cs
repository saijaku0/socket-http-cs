using HttpServer;

Server server = new(request => throw new Exception("boom"));
await server.StartAsync();