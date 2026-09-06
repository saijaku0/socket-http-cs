using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServer;

public class Server
{
    private readonly IPEndPoint _ipEndPoint;
    private readonly Func<HttpRequest, HttpResponse> _handler;

    public Server(
        Func<HttpRequest, HttpResponse> handler,
        string ipAddress = "127.0.0.1",
        int port = 8080)
    {
        if (!IPAddress.TryParse(ipAddress, out IPAddress? address))
            throw new ArgumentException("IPAddress is incorect");

        _ipEndPoint = new IPEndPoint(address, port);
        _handler = handler;
    }

    public async Task StartAsync()
    {
        using Socket socket = new(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);

        socket.Bind(_ipEndPoint);
        socket.Listen(10);
        Console.WriteLine($"[LISTEN] Listening on {_ipEndPoint}...");

        while (true)
        {
            try
            {
                using Socket client = await socket.AcceptAsync();

                byte[] bytes = new byte[1024];
                int bytesRead = client.Receive(bytes);
                if(bytesRead == 0)
                    continue;

                string data = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                HttpRequest request = HttpRequest.Parse(data);

                HttpResponse response;
                try
                {
                    response = _handler(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[HANDLER] Handler threw: {e.Message}");
                    response = new HttpResponse(500, "Internal Server Error");
                }

                client.Send(response.ToBytes());
                client.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Client processing failed: {e.Message}\n");
            }
        }
    }
}
