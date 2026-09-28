using HttpServer;

Server server = new(request => new HttpResponse(200, "Hello World"));
await server.StartAsync();