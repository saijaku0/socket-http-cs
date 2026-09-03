using System.Text;

namespace HttpServer;

public static class HttpResponse
{
    public static byte[] CreateResponse(string message, string contentType = "text/plain")
    {
        byte[] bodyBytes = Encoding.UTF8.GetBytes(message);
        string headers =    $"HTTP/1.1 200 OK\r\n" + 
                            $"Content-Type: {contentType}\r\n" + 
                            $"Content-Length: {bodyBytes.Length}\r\n\r\n";
        byte[] headerBytes = Encoding.UTF8.GetBytes(headers);
        return [.. headerBytes, .. bodyBytes];
    }
}
