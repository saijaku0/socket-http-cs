using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServer;

public class Server(int port)
{
    public async Task StartAsync()
    {
        IPEndPoint ipEndPoint = new(IPAddress.Any, port);
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            socket.Bind(ipEndPoint);
            Console.WriteLine($"[BIND] Bound to {ipEndPoint}");

            socket.Listen(10);
            Console.WriteLine($"[LISTEN] Listening on {ipEndPoint}...");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[FATAL] Failed to start server: {e.Message}");
            return;
        }

        while (true)
        {
            try
            {
                using Socket client = await socket.AcceptAsync();
                Console.WriteLine($"[CONNECT] Connected: {client.RemoteEndPoint}");

                byte[] bytes = new byte[1024];
                int bytesRead = client.Receive(bytes);
                if(bytesRead == 0)
                {
                    Console.WriteLine("[EMPTY] Received 0 bytes from client, closing connection.");
                    continue;
                }
                string data = Encoding.UTF8.GetString(bytes, 0, bytesRead);

                HttpRequest request = HttpRequest.Parse(data);
                Console.WriteLine($"[REQUEST] {request.Method} {request.Target} {request.Version}");

                byte[] responseBytes = HttpResponse.CreateResponse("Hello, World!");
                client.Send(responseBytes);

                client.Shutdown(SocketShutdown.Both);
                Console.WriteLine("[CLOSE] Connection with client is closed\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Client processing failed: {e.Message}\n");
            }
        }
    }
}
