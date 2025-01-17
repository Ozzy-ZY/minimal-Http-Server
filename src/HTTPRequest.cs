using System.Text;

namespace codecrafters_http_server;


public class HTTPRequest()
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string Version { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public string Body { get; set; }
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("HTTPRequest");
        sb.Append("\nMethod: " + Method);
        sb.Append("\nPath: " + Path);
        sb.Append("\nVersion: " + Version);
        foreach (var header in Headers.Keys)
        {
            sb.Append("\n" + header + ": " + Headers[header]);
        }
        sb.Append("\nBody: " + Body);
        return sb.ToString();
    }
}