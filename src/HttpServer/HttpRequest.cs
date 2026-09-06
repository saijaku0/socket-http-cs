using System.Collections.Immutable;

namespace HttpServer;

public record HttpRequest(
    string Method,
    string Target,
    string Version,
    ImmutableDictionary<string, string> Headers,
    string Body)
{
    public static HttpRequest Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Request cannot be empty");

        using var reader = new StringReader(raw);

        var requestLine = reader.ReadLine();
        if (requestLine is null)
            throw new ArgumentException("Invalid format HTTP");

        string[] parts = requestLine.Split(' ');
        if (parts.Length != 3)
            throw new ArgumentException("Invalid HTTP request line");

        var headersBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);
        string? headerLine;
        while (!string.IsNullOrWhiteSpace(headerLine = reader.ReadLine()))
        {
            var separatorIndex = headerLine.IndexOf(':');
            if (separatorIndex == -1) continue;
            var key = headerLine[..separatorIndex].Trim();
            var value = headerLine[(separatorIndex + 1)..].Trim();
            headersBuilder[key] = value;
        }

        string body = string.Empty;
        if (headersBuilder.TryGetValue("Content-Length", out var contentLengthValue)
            && int.TryParse(contentLengthValue, out var contentLength)
            && contentLength > 0)
        {
            char[] buffer = new char[contentLength];
            int charactersRead = reader.ReadBlock(buffer, 0, contentLength);
            body = new string(buffer, 0, charactersRead);
        }
        return new HttpRequest(parts[0], parts[1], parts[2], headersBuilder.ToImmutable(), body);
    }
}
