using System.Text;

namespace codecrafters_http_server;


public class HTTPRequest()
{
    public string MethodOrCode { get; set; } // eg. GET, POST or 403, 200
    public string PathOrMessage { get; set; } // eg. /echo or Forbidden, OK
    public string Version { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    public string Body { get; set; }
    
    public bool IsResponse{get;set;}
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (!IsResponse)
        {
            sb.Append($"{MethodOrCode} {PathOrMessage} {Version}");
        }
        else
        {
            sb.Append($"{Version} {MethodOrCode} {PathOrMessage}");
        }
        foreach (var header in Headers.Keys)
        {
            sb.Append("\r\n" + header + ": " + Headers[header]);
        }
        sb.Append($"\r\n\r\n+ {Body}");
        return sb.ToString();
    }
}