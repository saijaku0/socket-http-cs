using System.Text;

namespace HttpServer;

public record HttpResponse(
    int StatusCode,
    string Body,
    string ContentType = "text/plain")
{
    public byte[] ToBytes()
    {
        byte[] bodyBytes = Encoding.UTF8.GetBytes(Body);

        string statusText = StatusCode switch
        {
            200 => "OK",
            201 => "Created",
            400 => "Bad Request",
            404 => "Not Found",
            500 => "Internal Server Error",
            _ => "Unknown"
        };

        string headers =
            $"HTTP/1.1 {StatusCode} {statusText}\r\n" +
            $"Content-Type: {ContentType}\r\n" +
            $"Content-Length: {bodyBytes.Length}\r\n" +
            "\r\n";

        byte[] headerBytes = Encoding.UTF8.GetBytes(headers);

        return [.. headerBytes, .. bodyBytes];
    }
}