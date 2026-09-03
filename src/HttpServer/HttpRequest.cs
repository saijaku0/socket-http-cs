namespace HttpServer;

public record HttpRequest(string Method, string Target, string Version)
{
    public static HttpRequest Parse(string raw)
    {
        int lineEnd = raw.IndexOf("\r\n", StringComparison.Ordinal);
        string requestLine = lineEnd >= 0 ? raw[..lineEnd] : raw;

        string[] parts = requestLine.Split(' ');
        if (parts.Length != 3)
            throw new ArgumentException("Invalid HTTP request line");
        return new HttpRequest(parts[0], parts[1], parts[2]);
    }
}
