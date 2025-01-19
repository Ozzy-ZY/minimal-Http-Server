using System.Text;

namespace codecrafters_http_server;


public class HTTPRequest()
{
    public string MethodOrCode { get; set; } // eg. GET, POST or 403, 200
    public string PathOrMessage { get; set; } // eg. /echo or Forbidden, OK
    public string Version { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    public byte[] Body { get; set; }
    
    public bool IsResponse{get;set;}

    public byte[] GetResponse()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{Version} {MethodOrCode} {PathOrMessage}");
        foreach (var header in Headers.Keys)
        {
            sb.Append($"\r\n{header}: {Headers[header]}");
        }
        sb.Append("\r\n\r\n");
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Body == null)
        {
            return bytes;
        }
        using var memoryStream = new MemoryStream();
        memoryStream.Write(bytes, 0, bytes.Length);
        memoryStream.Write(Body, 0, Body.Length);
        var response = memoryStream.ToArray();
        //return [..bytes, ..Body];
        return response;
    }
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
            sb.Append($"\r\n{header}: {Headers[header]}");
        }
        sb.Append("\r\n\r\n");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Body != null)
        {
            sb.Append(Encoding.UTF8.GetString(Body));
        }
        return sb.ToString();
    }
}