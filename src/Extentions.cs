using System.IO.Compression;
using System.Text;

namespace codecrafters_http_server;

public static class Extentions
{
    public static HTTPRequest HandleBodyCompression(this HTTPRequest response)
    {
        // Gzip Encoding
        if (response.Headers.TryGetValue("Content-Encoding", out var header))
        {
            if (header.Contains("gzip"))
            {
                response.Headers["Content-Encoding"] = "gzip";
                response.Body = GzipCompress(Encoding.UTF8.GetString(response.Body));
                response.Headers["Content-Length"] = response.Body.Length.ToString();
                return response;
            }
        }
        return response;
    }

    private static byte[] GzipCompress(string data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var bytes = Encoding.UTF8.GetBytes(data);
        using var memoryStream = new MemoryStream();
        using var gzipStream =
            new GZipStream(memoryStream, CompressionMode.Compress, true);
        gzipStream.Write(bytes, 0, bytes.Length);
        gzipStream.Flush();
        gzipStream.Close();
        var compressed = memoryStream.ToArray();
        return compressed;
    }
}