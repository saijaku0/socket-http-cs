using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServer;

public class Server
{
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
                using NetworkStream stream = new(client, ownsSocket: false);
                byte[] bytes = await ReadBufferAsync(stream);
                if (bytes.Length == 0) continue;

                string data = Encoding.UTF8.GetString(bytes);
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
    private static async Task<byte[]> ReadBufferAsync(Stream inputStream, int bufferSize = 4096)
    {
        ArgumentNullException.ThrowIfNull(inputStream);

        using MemoryStream ms = new();
        byte[] buffer = new byte[bufferSize];
        int bytesRead;

        while ((bytesRead = await inputStream.ReadAsync(buffer)) > 0)
        {
            await ms.WriteAsync(buffer.AsMemory(0, bytesRead));
            if (ContainsEndMarker(ms, EndMarker)) break;
        }
        return ms.ToArray();
    }
    private static bool ContainsEndMarker(MemoryStream ms, byte[] marker)
    {
        long length = ms.Length;
        int markerLength = marker.Length;

        if (length < markerLength) return false;

        byte[] buffer = ms.GetBuffer();

        for (int i = 0; i <= length - markerLength; i++)
        {
            bool match = true;
            for (int j = 0; j < marker.Length; j++)
            {
                if (buffer[i + j] != marker[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return true;
        }

        return false;
    }
    private static readonly byte[] EndMarker = [ 13, 10, 13, 10 ];
    private readonly IPEndPoint _ipEndPoint;
    private readonly Func<HttpRequest, HttpResponse> _handler;
}
